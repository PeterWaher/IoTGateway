﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Json;
using Waher.Content.Text;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.HTTP.HTTP2;
using Waher.Networking.HTTP.TransferEncodings;
using Waher.Networking.HTTP.WebSockets;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;
using Waher.Security;
#if WINDOWS_UWP
using Windows.Networking.Sockets;
#endif

namespace Waher.Networking.HTTP
{
	internal enum ConnectionMode
	{
		Http,
		Http2Init,
		Http2Live,
		WebSocket
	}

	/// <summary>
	/// Class managing a remote client connection to a local <see cref="HttpServer"/>.
	/// </summary>
	internal class HttpClientConnection : CommunicationLayer, IDisposable
	{
		internal const byte CR = 13;
		internal const byte LF = 10;
		internal const int MaxHeaderSize = 65536;
		internal const int MaxInmemoryMessageSize = 1024 * 1024;    // 1 MB
		internal const long MaxEntitySize = 1024 * 1024 * 1024;     // 1 GB

		private Guid id = Guid.NewGuid();
		private MemoryStream headerStream = null;
		private Stream dataStream = null;
		private TransferEncoding transferEncoding = null;
		private readonly HttpServer server;
		private BinaryTcpClient client;
		private HttpRequestHeader header = null;
		private ConnectionMode mode = ConnectionMode.Http;
		private ConnectionSettings settings = null;
		private WebSocket webSocket = null;
		private Encoding rxEncoding = null;
		private HTTP2.BinaryReader reader = null;
		private byte b1 = 0;
		private byte b2 = 0;
		private byte b3 = 0;
		private readonly bool encrypted;
		private bool disposed = false;
		private bool rxText = false;

		internal HttpClientConnection(HttpServer Server, BinaryTcpClient Client, bool Encrypted, params ISniffer[] Sniffers)
			: base(false, Sniffers)
		{
			this.server = Server;
			this.client = Client;
			this.encrypted = Encrypted;

			this.client.OnDisconnected += this.Client_OnDisconnected;
			this.client.OnError += this.Client_OnError;
			this.client.OnReceived += this.Client_OnReceived;
		}

		private Task<bool> Client_OnReceived(object Sender, byte[] Buffer, int Offset, int Count)
		{
			this.server.DataReceived(Count);

			switch (this.mode)
			{
				case ConnectionMode.Http:
					if (this.header is null)
						return this.BinaryHeaderReceived(Buffer, Offset, Count);
					else
						return this.BinaryDataReceived(Buffer, Offset, Count);

				case ConnectionMode.Http2Init:
					return this.BinaryHttp2InitDataReceived(Buffer, Offset, Count);

				case ConnectionMode.Http2Live:
					return this.BinaryHttp2LiveDataReceived(Buffer, Offset, Count);

				case ConnectionMode.WebSocket:
					return this.webSocket?.WebSocketDataReceived(Buffer, Offset, Count) ?? Task.FromResult(false);

				default:
					return Task.FromResult(false);
			}
		}

		private Task Client_OnError(object Sender, Exception Exception)
		{
			return this.DisposeAsync();
		}

		private Task Client_OnDisconnected(object Sender, EventArgs e)
		{
			return this.DisposeAsync();
		}

		/// <summary>
		/// Closes the connection and disposes of all resources.
		/// </summary>
		[Obsolete("Use the DisposeAsync() method.")]
		public async void Dispose()
		{
			try
			{
				await this.DisposeAsync();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Closes the connection and disposes of all resources.
		/// </summary>
		public async Task DisposeAsync()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				if (!(this.webSocket is null))
				{
					await this.webSocket.DisposeAsync();
					this.webSocket = null;
				}

				this.headerStream?.Dispose();
				this.headerStream = null;

				this.dataStream?.Dispose();
				this.dataStream = null;

				this.client?.DisposeWhenDone();
				this.client = null;

				this.server.Remove(this);
			}
		}

		internal Guid Id => this.id;

		internal HttpServer Server => this.server;

		internal bool Disposed => this.disposed;

		internal BinaryTcpClient Client => this.client;

#if !WINDOWS_UWP
		internal bool Encrypted => this.encrypted;
#endif

		private async Task<bool> BinaryHeaderReceived(byte[] Data, int Offset, int NrRead)
		{
			string Header;
			int i, c;
			byte b;

			c = Offset + NrRead;

			for (i = Offset; i < c; i++)
			{
				b = Data[i];

				if (this.b1 == CR && this.b2 == LF && this.b3 == CR && b == LF) // RFC 2616, §2.2
				{
					if (this.headerStream is null)
						Header = InternetContent.ISO_8859_1.GetString(Data, Offset, i - Offset - 3);
					else
					{
						this.headerStream.Write(Data, Offset, i - Offset - 3);

						Header = InternetContent.ISO_8859_1.GetString(this.headerStream.ToArray(), 0, (int)this.headerStream.Position);
						this.headerStream = null;
					}
				}
				else if (this.b3 == LF && b == LF)  // RFC 2616, §19.3
				{
					if (this.headerStream is null)
						Header = InternetContent.ISO_8859_1.GetString(Data, Offset, i - Offset - 1);
					else
					{
						this.headerStream.Write(Data, Offset, i - Offset - 1);
						Header = InternetContent.ISO_8859_1.GetString(this.headerStream.ToArray(), 0, (int)this.headerStream.Position);
						this.headerStream = null;
					}
				}
				else
				{
					this.b1 = this.b2;
					this.b2 = this.b3;
					this.b3 = b;
					continue;
				}

				this.header = new HttpRequestHeader(Header, this.server.VanityResources, this.encrypted ? "https" : "http");

				if (this.HasSniffers)
				{
					await this.ReceiveText(Header);

					HttpFieldContentType ContentType = this.header.ContentType;
					if (ContentType is null)
						this.rxText = false;
					else
					{
						if (!string.IsNullOrEmpty(ContentType.CharacterSet))
						{
							this.rxText = true;
							this.rxEncoding = ContentType.Encoding;
						}
						else
						{
							if (IsSniffableTextType(ContentType.Value))
							{
								this.rxText = true;
								this.rxEncoding = Encoding.UTF8;
							}
							else
								this.rxText = false;
						}
					}
				}

				if (this.header.HttpVersion < 1)
				{
					await this.SendResponse(null, null, new HttpException(505, "HTTP Version Not Supported",
						"At least HTTP Version 1.0 is required."), true);
					return false;
				}

				if (!(this.header.ContentLength is null) && (this.header.ContentLength.ContentLength > MaxEntitySize))
				{
					await this.SendResponse(null, null, new HttpException(413, "Method Not Allowed", "Empty method."), true);
					return false;
				}

				if (string.IsNullOrEmpty(this.header.Method))
				{
					await this.SendResponse(null, null, new HttpException(405, "Method Not Allowed", "Empty method."), true);
					return false;
				}

				foreach (char ch in this.header.Method)
				{
					if (!((ch >= 'A' && ch <= 'Z') || ch == '-' || ch == '_'))
					{
						await this.SendResponse(null, null, new HttpException(405, "Method Not Allowed", "Invalid method."), true);
						return false;
					}
				}

				if (this.header.Method == "PRI")
				{
					this.mode = ConnectionMode.Http2Init;
					this.settings = new ConnectionSettings();

					if (i + 1 < NrRead)
						return await this.BinaryHttp2InitDataReceived(Data, i + 1, NrRead - i - 1);
					else
						return true;
				}
				else
				{
					if (!(this.header.Upgrade is null) &&
						this.header.Upgrade.Value == "h2c" &&
						!(this.header.Connection is null) &&
						this.header.Connection.Value.StartsWith("Upgrade") &&
						this.header.TryGetHeaderField("HTTP2-Settings", out HttpField Http2Settings))
					{
						try
						{
							byte[] Bin = Convert.FromBase64String(Http2Settings.Value);
							if (ConnectionSettings.TryParse(Bin, out ConnectionSettings Settings))
							{
								this.settings = Settings;
								this.mode = ConnectionMode.Http2Live;

								using HttpResponse Response = new HttpResponse(this.client, this, this.server, null)
								{
									StatusCode = 101,
									StatusMessage = "Switching Protocols",
									ContentLength = null,
									ContentType = null,
									ContentLanguage = null
								};

								Response.SetHeader("Upgrade", "h2c");
								Response.SetHeader("Connection", "Upgrade");

								await Response.SendResponse();

								if (i + 1 < NrRead)
									return await this.BinaryHttp2LiveDataReceived(Data, i + 1, NrRead - i - 1);
								else
									return true;
							}
						}
						catch (Exception)
						{
							// Ignore
						}
					}

					if (i + 1 < NrRead)
						return await this.BinaryDataReceived(Data, i + 1, NrRead - i - 1);

					if (!this.header.HasMessageBody)
						return await this.RequestReceived();

					return true;
				}
			}

			this.headerStream ??= new MemoryStream();
			this.headerStream.Write(Data, Offset, NrRead);

			if (this.headerStream.Position < MaxHeaderSize)
				return true;
			else
			{
				if (this.HasSniffers)
				{
					int d = (int)this.headerStream.Position;
					byte[] Data2 = new byte[d];
					this.headerStream.Position = 0;
					await this.headerStream.ReadAllAsync(Data2, 0, d);
					await this.ReceiveBinary(Data2);
				}

				await this.SendResponse(null, null, new HttpException(431, "Request Header Fields Too Large",
					"Max Header Size: " + MaxHeaderSize.ToString()), true);
				return false;
			}
		}

		private static readonly char[] http2Preface = new char[] { 'S', 'M', '\r', '\n', '\r', '\n' };

		private async Task<bool> BinaryHttp2InitDataReceived(byte[] Data, int Offset, int NrRead)
		{
			int i, c;
			byte b;

			c = Offset + NrRead;

			for (i = Offset; i < c; i++)
			{
				b = Data[i];

				if (b != http2Preface[this.settings.InitStep++])
				{
					if (this.HasSniffers && i > Offset)
						await this.ReceiveText(InternetContent.ISO_8859_1.GetString(Data, Offset, i - Offset));

					await this.SendResponse(null, null, new HttpException(405, "Method Not Allowed", "Invalid HTTP/2 connection preface."), true);
					return false;
				}

				if (this.settings.InitStep == 6)
				{
					this.mode = ConnectionMode.Http2Live;

					if (this.HasSniffers)
						await this.ReceiveText("\r\nSM\r\n");

					if (i + 1 < NrRead)
						return await this.BinaryHttp2LiveDataReceived(Data, i + 1, NrRead - i - 1);
					else
						return true;
				}
			}

			return true;
		}

		private async Task<bool> BinaryHttp2LiveDataReceived(byte[] Data, int Offset, int NrRead)
		{
			int End = Offset + NrRead;

			while (Offset < End)
			{
				switch (this.http2State)
				{
					case 0:
						this.http2FrameLength = Data[Offset++];
						this.http2State++;
						break;

					case 1:
					case 2:
						this.http2FrameLength <<= 8;
						this.http2FrameLength |= Data[Offset++];
						this.http2State++;
						break;

					case 3:
						this.http2FrameType = (FrameType)Data[Offset++];
						this.http2State++;
						break;

					case 4:
						this.http2FrameFlags = Data[Offset++];
						this.http2State++;
						break;

					case 5:
						this.http2StreamId = Data[Offset++] & 127;
						this.http2State++;
						break;

					case 6:
					case 7:
						this.http2StreamId <<= 8;
						this.http2StreamId |= Data[Offset++];
						this.http2State++;
						break;

					case 8:
						this.http2StreamId <<= 8;
						this.http2StreamId |= Data[Offset++];

						if (this.http2FrameLength == 0)
						{
							// TODO: Process frame.
							this.http2State = 0;
						}
						else
						{
							this.http2FramePos = 0;

							if (this.http2FrameLength > this.settings.MaxFrameSize)
								this.http2State += 2;
							else
							{
								this.http2Frame ??= new byte[this.settings.MaxFrameSize];
								this.http2State++;
							}
						}
						break;

					case 9:
						int i = Math.Min(End - Offset, this.http2FrameLength - this.http2FramePos);
						Array.Copy(Data, Offset, this.http2Frame, this.http2FramePos, i);
						Offset += i;
						this.http2FramePos += i;

						if (this.http2FramePos >= this.http2FrameLength)
						{
							this.http2State = 0;

							if (!await this.ProcessHttp2Frame())
								return false;
						}
						break;

					case 10:
						i = Math.Min(End - Offset, this.http2FrameLength - this.http2FramePos);
						Offset += i;
						this.http2FramePos += i;

						if (this.http2FramePos >= this.http2FrameLength)
						{
							this.http2State = 0;

							if (!await this.ReturnHttp2Error(Http2Error.FrameSizeError, true))
								return false;
						}
						break;

					default:
						return false;
				}
			}

			return true;    // TODO
		}

		private int http2State = 0;
		private int http2FrameLength = 0;
		private int http2StreamId = 0;
		private int http2FramePos = 0;
		private FrameType http2FrameType = 0;
		private byte http2FrameFlags = 0;
		private byte[] http2Frame = null;

		private async Task<bool> ProcessHttp2Frame()
		{
			if (this.HasSniffers)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.http2FrameType.ToString());
				sb.Append(", Flags ");
				sb.Append(this.http2FrameFlags.ToString("X2"));
				sb.Append(", Stream ");
				sb.Append(this.http2StreamId.ToString());
				sb.Append(", Size ");
				sb.Append(this.http2FrameLength.ToString());
				sb.Append(" bytes");

				await this.Information(sb.ToString());
			}

			if (this.reader is null)
				this.reader = new HTTP2.BinaryReader(this.http2Frame, 0, this.http2FrameLength);
			else
				this.reader.Reset(this.http2Frame, 0, this.http2FrameLength);

			switch (this.http2FrameType)
			{
				case FrameType.Data:
					// TODO
					break;

				case FrameType.Headers:
					// TODO
					break;

				case FrameType.Priority:
					// TODO
					break;

				case FrameType.ResetStream:
					// TODO
					break;

				case FrameType.Settings:
					// SETTINGS, §6.5, RFC 7540: https://datatracker.ietf.org/doc/html/rfc7540#section-6.5

					if (this.http2StreamId != 0)
						return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

					if ((this.http2FrameFlags & 1) != 0)    // ACK
					{
						if (this.http2FrameLength > 0)
							return await this.ReturnHttp2Error(Http2Error.FrameSizeError, true);
					}
					else
					{
						while (this.reader.HasMore)
						{
							if (this.reader.BytesLeft < 6)
								return await this.ReturnHttp2Error(Http2Error.FrameSizeError, true);

							ushort Key = this.reader.NextUInt16();
							uint Value = this.reader.NextUInt32();

							switch (Key)
							{
								case 1:
									if (this.HasSniffers)
										await this.Information("SETTINGS_HEADER_TABLE_SIZE = " + Value.ToString());

									this.settings.HeaderTableSize = Value;
									break;

								case 2:
									if (this.HasSniffers)
										await this.Information("SETTINGS_ENABLE_PUSH = " + Value.ToString());

									if (Value > 1)
										return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

									this.settings.EnablePush = Value != 0;
									break;

								case 3:
									if (this.HasSniffers)
										await this.Information("SETTINGS_MAX_CONCURRENT_STREAMS = " + Value.ToString());

									this.settings.MaxConcurrentStreams = Value;
									break;

								case 4:
									if (this.HasSniffers)
										await this.Information("SETTINGS_INITIAL_WINDOW_SIZE = " + Value.ToString());

									if (Value > 0x7fffffff)
										return await this.ReturnHttp2Error(Http2Error.FlowControlError, true);

									this.settings.InitialWindowSize = Value;
									break;

								case 5:
									if (this.HasSniffers)
										await this.Information("SETTINGS_MAX_FRAME_SIZE = " + Value.ToString());

									if (Value > 0x00ffffff)
										return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

									this.settings.MaxFrameSize = Value;
									break;

								case 6:
									if (this.HasSniffers)
										await this.Information("SETTINGS_MAX_HEADER_LIST_SIZE = " + Value.ToString());

									this.settings.MaxHeaderListSize = Value;
									break;

								default:
									if (this.HasSniffers)
										await this.Information("(" + Key.ToString() + ", " + Value.ToString() + ")");

									break;  // Ignore
							}

							// TODO: Return Settings(Ack=1) frame
						}
					}
					break;

				case FrameType.PushPromise:
					// TODO
					break;

				case FrameType.Ping:
					// TODO
					break;

				case FrameType.GoAway:
					// TODO
					break;

				case FrameType.WindowUpdate:
					// WINDOW_UPDATE, §6.10, RFC 7540: https://datatracker.ietf.org/doc/html/rfc7540#section-6.9
					// TODO
					break;

				case FrameType.Continuation:
					// TODO
					break;
			}

			// TODO: process frame and return response.
			return true;
		}

		private async Task<bool> ReturnHttp2Error(Http2Error ErrorCode, bool ConnectionError)
		{
			if (this.HasSniffers)
				await this.Error(ErrorCode.ToString());

			// TODO: Return frame.
			return !ConnectionError;
		}

		internal static bool IsSniffableTextType(string ContentType)
		{
			ContentType = ContentType.ToLower();
			int j = ContentType.IndexOf('/');
			if (j < 0)
				return false;

			string s = ContentType[..j];

			// TODO: Customizable.

			switch (s)
			{
				case "text":
					switch (ContentType)
					{
						case PlainTextCodec.DefaultContentType:
						case CsvCodec.ContentType:
						case "text/x-json":
						case "text/tab-separated-values":
						case "text/xml":
						default:
							return true;

						case "text/css":
						case "text/x-cssx":
						case "text/html":
						case "text/markdown":
						case "text/xsl":
							return false;
					}

				case "application":
					switch (ContentType)
					{
						case "application/x-www-form-urlencoded":
						case JsonCodec.DefaultContentType:
						case "application/xml":
						case "application/link-format":
						case "application/vnd.oma.lwm2m+json":
						case "application/vnd.oma.lwm2m+tlv":
						case "application/jose+json":
						case "application/jwt":
							return true;

						case "application/javascript":
						case "application/xhtml+xml":
						case "application/xslt+xml":
						default:
							return false;
					}

				case "multipart":
					return ContentType switch
					{
						"multipart/form-data" => true,
						_ => false,
					};

				default:
					return false;
			}
		}

		private async Task<bool> BinaryDataReceived(byte[] Data, int Offset, int NrRead)
		{
			if (this.dataStream is null)
			{
				if (!(this.header.ContentEncoding is null))     // TODO: Support Content-Encoding in POST, PUT and PATCH, etc.
				{
					await this.SendResponse(null, null, new HttpException(UnsupportedMediaTypeException.Code,
						UnsupportedMediaTypeException.StatusMessage, "Content-Encoding not supported."), false);
					return true;
				}

				HttpFieldTransferEncoding TransferEncoding = this.header.TransferEncoding;
				if (!(TransferEncoding is null))
				{
					if (TransferEncoding.Value == "chunked")
					{
						this.dataStream = new TemporaryStream();
						this.transferEncoding = new ChunkedTransferEncoding(new BinaryOutputStream(this.dataStream), null, false, null);
					}
					else
					{
						await this.SendResponse(null, null, new HttpException(NotImplementedException.Code,
							NotImplementedException.StatusMessage, "Transfer encoding not implemented."), false);
						return true;
					}
				}
				else
				{
					HttpFieldContentLength ContentLength = this.header.ContentLength;
					if (!(ContentLength is null))
					{
						long l = ContentLength.ContentLength;
						if (l < 0)
						{
							await this.SendResponse(null, null, new HttpException(BadRequestException.Code,
								BadRequestException.StatusMessage, "Negative content lengths invalid."), false);
							return true;
						}

						if (l <= MaxInmemoryMessageSize)
							this.dataStream = new MemoryStream((int)l);
						else
							this.dataStream = new TemporaryStream();

						this.transferEncoding = new ContentLengthEncoding(new BinaryOutputStream(this.dataStream), l, null, false, null);
					}
					else
					{
						await this.SendResponse(null, null, new HttpException(411, "Length Required",
							"Content Length required."), true);
						return false;
					}
				}
			}

			ulong DecodingResponse = await this.transferEncoding.DecodeAsync(Data, Offset, NrRead);
			int NrAccepted = (int)DecodingResponse;
			bool Complete = (DecodingResponse & 0x100000000) != 0;

			if (this.HasSniffers)
			{
				if (Offset == 0 && NrAccepted == Data.Length)
				{
					if (this.rxText)
						await this.ReceiveText(this.rxEncoding.GetString(Data));
					else
						await this.ReceiveBinary(Data);
				}
				else
				{
					if (this.rxText)
						await this.ReceiveText(this.rxEncoding.GetString(Data, Offset, NrAccepted));
					else
					{
						byte[] Data2 = new byte[NrAccepted];
						Array.Copy(Data, Offset, Data2, 0, NrAccepted);
						await this.ReceiveBinary(Data2);
					}
				}
			}

			if (Complete)
			{
				if (this.transferEncoding.InvalidEncoding)
				{
					await this.SendResponse(null, null, new HttpException(BadRequestException.Code,
						BadRequestException.StatusMessage, "Invalid transfer encoding."), false);
					return true;
				}
				else if (this.transferEncoding.TransferError)
				{
					await this.SendResponse(null, null, new HttpException(InternalServerErrorException.Code,
						InternalServerErrorException.StatusMessage, "Unable to transfer content to resource."), false);
					return true;
				}
				else
				{
					Offset += NrAccepted;
					NrRead -= NrAccepted;

					if (!await this.RequestReceived())
						return false;

					if (NrRead > 0)
						return await this.BinaryHeaderReceived(Data, Offset, NrRead);
					else
						return true;
				}
			}
			else if (this.dataStream.Position > MaxEntitySize)
			{
				this.dataStream.Dispose();
				this.dataStream = null;

				await this.SendResponse(null, null, new HttpException(413, "Request Entity Too Large",
					"Maximum Entity Size: " + MaxEntitySize.ToString()), true);
				return false;
			}
			else
				return true;
		}

		private async Task<bool> RequestReceived()
		{
#if WINDOWS_UWP
			StreamSocket UnderlyingSocket = this.client.Client;
			HttpRequest Request = new HttpRequest(this.server, this.header, this.dataStream,
				UnderlyingSocket.Information.RemoteAddress.ToString() + ":" + UnderlyingSocket.Information.RemotePort,
				UnderlyingSocket.Information.LocalAddress.ToString() + ":" + UnderlyingSocket.Information.LocalPort);
#else
			Socket UnderlyingSocket = this.client.Client.Client;
			HttpRequest Request = new HttpRequest(this.server, this.header, this.dataStream,
				UnderlyingSocket.RemoteEndPoint.ToString(),
				UnderlyingSocket.LocalEndPoint.ToString());
#endif
			Request.clientConnection = this;

			bool? Queued = await this.QueueRequest(Request);

			if (Queued.HasValue)
			{
				if (!Queued.Value && !(this.dataStream is null))
					this.dataStream.Dispose();

				this.header = null;
				this.dataStream = null;
				this.transferEncoding = null;

				return Queued.Value;
			}
			else
				return true;
		}

		private async Task<bool?> QueueRequest(HttpRequest Request)
		{
			HttpAuthenticationScheme[] AuthenticationSchemes;
			bool Result;

			try
			{
				if (NetworkingModule.Stopping)
				{
					await this.SendResponse(Request, null, new ServiceUnavailableException("Service is shutting down. Please try again later."), true,
						new KeyValuePair<string, string>("Retry-After", "300"));    // Try again in 5 minutes.
					Result = false;
				}
				else if (this.server.TryGetResource(Request.Header.Resource, true, out HttpResource Resource, out string SubPath))
				{
					Request.Resource = Resource;
					Request.SubPath = SubPath;
#if WINDOWS_UWP
					this.server.RequestReceived(Request, this.client.Client.Information.RemoteAddress.ToString() + ":" + 
						this.client.Client.Information.RemotePort, Resource, SubPath);
#else
					this.server.RequestReceived(Request, this.client.Client.Client.RemoteEndPoint.ToString(), Resource, SubPath);
#endif

					AuthenticationSchemes = Resource.GetAuthenticationSchemes(Request);
					if (!(AuthenticationSchemes is null) && AuthenticationSchemes.Length > 0 && Request.Header.Method != "OPTIONS")
					{
						ILoginAuditor Auditor = this.server.LoginAuditor;

						if (!(Auditor is null))
						{
							DateTime? Next = await Auditor.GetEarliestLoginOpportunity(Request.RemoteEndPoint, "HTTP");

							if (Next.HasValue)
							{
								StringBuilder sb = new StringBuilder();
								DateTime TP = Next.Value;
								DateTime Today = DateTime.Today;
								HttpException Error;

								if (Next.Value == DateTime.MaxValue)
								{
									sb.Append("This endpoint (");
									sb.Append(Request.RemoteEndPoint);
									sb.Append(") has been blocked from the system.");

									Error = new ForbiddenException(sb.ToString());
								}
								else
								{
									sb.Append("Too many failed login attempts in a row registered. Try again in ");

									TimeSpan Span = TP - DateTime.Now;
									double d;

									if ((d = Span.TotalDays) >= 1)
									{
										d = Math.Ceiling(d);
										sb.Append(d.ToString());
										sb.Append(" day");
									}
									else if ((d = Span.TotalHours) >= 1)
									{
										d = Math.Ceiling(d);
										sb.Append(d.ToString());
										sb.Append(" hour");
									}
									else
									{
										d = Math.Ceiling(Span.TotalMinutes);
										sb.Append(d.ToString());
										sb.Append(" minute");
									}

									if (d > 1)
										sb.Append('s');

									sb.Append(". Remote Endpoint: ");
									sb.Append(Request.RemoteEndPoint);

									Error = new TooManyRequestsException(sb.ToString());
								}

								await this.SendResponse(Request, null, Error, true);
								Request.Dispose();
								return true;
							}
						}

						foreach (HttpAuthenticationScheme Scheme in AuthenticationSchemes)
						{
							if (Scheme.UserSessions && Request.Session is null)
							{
								string HttpSessionID = HttpResource.GetSessionId(Request, Request.Response);

								if (!string.IsNullOrEmpty(HttpSessionID))
									Request.Session = this.server.GetSession(HttpSessionID);

								if (Request.Session is null)
								{
									Request.Session = HttpServer.CreateVariables();
									Request.tempSession = true;
								}
							}

							IUser User = await Scheme.IsAuthenticated(Request);
							if (!(User is null))
							{
								Request.User = User;
								break;
							}
						}

						if (Request.User is null)
						{
							List<KeyValuePair<string, string>> Challenges = new List<KeyValuePair<string, string>>();
							bool Encrypted = this.client.IsEncrypted;
#if !WINDOWS_UWP
							int Strength = Encrypted ? Math.Min(
								this.client.CipherStrength, this.client.KeyExchangeStrength) : 0;
#endif

							foreach (HttpAuthenticationScheme Scheme in AuthenticationSchemes)
							{
								if (Scheme.RequireEncryption)
								{
									if (!Encrypted)
										continue;

#if !WINDOWS_UWP
									if (Scheme.MinStrength > Strength)
										continue;
#endif
								}

								string Challenge = Scheme.GetChallenge();
								if (!string.IsNullOrEmpty(Challenge))
									Challenges.Add(new KeyValuePair<string, string>("WWW-Authenticate", Challenge));
							}

							await this.SendResponse(Request, null, new HttpException(401, UnauthorizedException.StatusMessage,
								(await Resource.DefaultErrorContent(401)) ?? "Unauthorized access prohibited."), false, Challenges.ToArray());
							Request.Dispose();
							return true;
						}
					}

					Resource.Validate(Request);

					if (!(Request.Header.Expect is null))
					{
						if (Request.Header.Expect.Continue100)
						{
							if (!Request.HasData)
							{
								await this.SendResponse(Request, null, new HttpException(100, "Continue", null), false);
								return null;
							}
						}
						else
						{
							await this.SendResponse(Request, null, new HttpException(417, "Expectation Failed",
								(await Resource.DefaultErrorContent(417)) ?? "Unable to parse Expect header."), true);
							Request.Dispose();
							return false;
						}
					}

					Task _ = Task.Run(() => this.ProcessRequest(Request, Resource));
					return true;
				}
				else
				{
					await this.SendResponse(Request, null, new NotFoundException("Resource not found: " + this.server.CheckResourceOverride(Request.Header.Resource)), false);
					Result = true;
				}
			}
			catch (HttpException ex)
			{
				Result = Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData;
				await this.SendResponse(Request, null, ex, !Result, ex.HeaderFields);
			}
			catch (System.NotImplementedException ex)
			{
				Result = Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData;

				await this.Exception(ex);

				await this.SendResponse(Request, null, new NotImplementedException(ex.Message), !Result);
			}
			catch (IOException ex)
			{
				await this.Exception(ex);

				int Win32ErrorCode = ex.HResult & 0xFFFF;
				if (Win32ErrorCode == 0x27 || Win32ErrorCode == 0x70)   // ERROR_HANDLE_DISK_FULL, ERROR_DISK_FULL
				{
					await this.SendResponse(Request, null, new HttpException(InsufficientStorageException.Code,
						InsufficientStorageException.StatusMessage, "Insufficient space."), true);
				}
				else
					await this.SendResponse(Request, null, new InternalServerErrorException(ex.Message), true);

				Result = false;
			}
			catch (XmlException ex)
			{
				Result = Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData;

				ex = XML.AnnotateException(ex);
				await this.Exception(ex);

				await this.SendResponse(Request, null, new InternalServerErrorException(ex.Message), !Result);
			}
			catch (Exception ex)
			{
				Result = Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData;

				await this.Exception(ex);

				await this.SendResponse(Request, null, new InternalServerErrorException(ex.Message), !Result);
			}

			Request.Dispose();
			return Result;
		}

		private KeyValuePair<string, string>[] Merge(KeyValuePair<string, string>[] Headers, LinkedList<Cookie> Cookies)
		{
			if (Cookies is null || Cookies.First is null)
				return Headers;

			List<KeyValuePair<string, string>> Result = new List<KeyValuePair<string, string>>();
			Result.AddRange(Headers);

			foreach (Cookie Cookie in Cookies)
				Result.Add(new KeyValuePair<string, string>("Set-Cookie", Cookie.ToString()));

			return Result.ToArray();
		}

		private async Task ProcessRequest(HttpRequest Request, HttpResource Resource)
		{
			HttpResponse Response = null;

			try
			{
#if !WINDOWS_UWP
				HttpRequestHeader Header = Request.Header;
				int? UpgradePort = null;

				if (!this.encrypted &&
					(Header.UpgradeInsecureRequests?.Upgrade ?? false) &&
					!(Header.Host is null) &&
					string.Compare(Header.Host.Value, "localhost", true) != 0 &&
					(UpgradePort = this.server.UpgradePort).HasValue)
				{
					StringBuilder Location = new StringBuilder();
					string s;
					int i;

					Location.Append("https://");

					s = Header.Host.Value;
					i = s.IndexOf(':');
					if (i > 0)
						s = s[..i];

					Location.Append(s);

					if (!(UpgradePort is null) && UpgradePort.Value != HttpServer.DefaultHttpsPort)
					{
						Location.Append(':');
						Location.Append(UpgradePort.Value.ToString());
					}

					Location.Append(Header.Resource);

					if (!string.IsNullOrEmpty(s = Header.QueryString))
					{
						Location.Append('?');
						Location.Append(Header.QueryString);
					}

					if (!string.IsNullOrEmpty(s = Header.Fragment))
					{
						Location.Append('#');
						Location.Append(Header.Fragment);
					}

					await this.SendResponse(Request, Response, new HttpException(TemporaryRedirectException.Code, TemporaryRedirectException.StatusMessage,
						new KeyValuePair<string, string>("Location", Location.ToString()),
						new KeyValuePair<string, string>("Vary", "Upgrade-Insecure-Requests")), false);
				}
				else
#endif
				{
					Response = new HttpResponse(this.client, this, this.server, Request);
					await Resource.Execute(this.server, Request, Response);
				}
			}
			catch (HttpException ex)
			{
				if (Response is null || !Response.HeaderSent)
				{
					try
					{
						await this.SendResponse(Request, Response, ex, false, this.Merge(ex.HeaderFields, Response.Cookies));
					}
					catch (Exception)
					{
						this.CloseStream();
					}
				}
				else
					this.CloseStream();
			}
			catch (System.NotImplementedException ex)
			{
				await this.Exception(ex);

				if (Response is null || !Response.HeaderSent)
				{
					try
					{
						await this.SendResponse(Request, Response, new NotImplementedException(ex.Message), true);
					}
					catch (Exception)
					{
						this.CloseStream();
					}
				}
				else
					this.CloseStream();
			}
			catch (IOException ex)
			{
				await this.Exception(ex);

				if (Response is null || !Response.HeaderSent)
				{
					try
					{
						int Win32ErrorCode = ex.HResult & 0xFFFF;
						if (Win32ErrorCode == 0x27 || Win32ErrorCode == 0x70)   // ERROR_HANDLE_DISK_FULL, ERROR_DISK_FULL
						{
							await this.SendResponse(Request, null, new HttpException(InsufficientStorageException.Code,
								InsufficientStorageException.StatusMessage, "Insufficient space."), true);
						}
						else
							await this.SendResponse(Request, null, new InternalServerErrorException(ex.Message), true);
					}
					catch (Exception)
					{
						this.CloseStream();
					}
				}
				else
					this.CloseStream();
			}
			catch (XmlException ex)
			{
				ex = XML.AnnotateException(ex);
				await this.Exception(ex);

				if (Response is null || !Response.HeaderSent)
				{
					try
					{
						await this.SendResponse(Request, Response, new InternalServerErrorException(ex.Message), true);
					}
					catch (Exception)
					{
						this.CloseStream();
					}
				}
				else
					this.CloseStream();
			}
			catch (Exception ex)
			{
				await this.Exception(ex);

				if (Response is null || !Response.HeaderSent)
				{
					try
					{
						await this.SendResponse(Request, Response, new InternalServerErrorException(ex.Message), true);
					}
					catch (Exception)
					{
						this.CloseStream();
					}
				}
				else
					this.CloseStream();
			}
			finally
			{
				Request.Dispose();
			}
		}

		private void CloseStream()
		{
			this.client?.DisposeWhenDone();
			this.client = null;
		}

		private async Task SendResponse(HttpRequest Request, HttpResponse Response, HttpException ex, bool CloseAfterTransmission,
			params KeyValuePair<string, string>[] HeaderFields)
		{
			bool DisposeResponse;

			if (Response is null)
			{
				Response = new HttpResponse(this.client, this, this.server, Request)
				{
					StatusCode = ex.StatusCode,
					StatusMessage = ex.Message,
					ContentLength = null,
					ContentType = null,
					ContentLanguage = null
				};

				DisposeResponse = true;
			}
			else
			{
				Response.StatusCode = ex.StatusCode;
				Response.StatusMessage = ex.Message;
				Response.ContentLength = null;
				Response.ContentType = null;
				Response.ContentLanguage = null;

				DisposeResponse = false;
			}

			try
			{
				foreach (KeyValuePair<string, string> P in HeaderFields)
					Response.SetHeader(P.Key, P.Value);

				if (CloseAfterTransmission)
				{
					Response.CloseAfterResponse = true;
					Response.SetHeader("Connection", "close");
				}

				if (ex is null)
					await Response.SendResponse();
				else
					await Response.SendResponse(ex);
			}
			finally
			{
				if (DisposeResponse)
					await Response.DisposeAsync();
			}
		}

		internal void Upgrade(WebSocket Socket)
		{
			this.mode = ConnectionMode.WebSocket;
			this.webSocket = Socket;
		}

		/// <summary>
		/// Checks if the connection is live.
		/// </summary>
		/// <returns>If the connection is still live.</returns>
		internal bool CheckLive()
		{
			try
			{
				if (this.disposed)
					return false;

				if (!this.client.Connected)
					return false;

#if WINDOWS_UWP
				return true;
#else

				// https://msdn.microsoft.com/en-us/library/system.net.sockets.socket.connected.aspx

				Socket Socket = this.client.Client.Client;
				bool BlockingBak = Socket.Blocking;
				try
				{
					byte[] Temp = new byte[1];

					Socket.Blocking = false;
					Socket.Send(Temp, 0, 0);

					return true;
				}
				catch (SocketException ex)
				{
					int Win32ErrorCode = ex.HResult & 0xFFFF;

					if (Win32ErrorCode == 10035)    // WSAEWOULDBLOCK
						return true;
					else
						return false;
				}
				finally
				{
					Socket.Blocking = BlockingBak;
				}
#endif
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
