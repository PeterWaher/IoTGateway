//#define INFO_IN_SNIFFERS

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
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
using Waher.Runtime.IO;
using Waher.Runtime.Temporary;
using Waher.Security;
#if WINDOWS_UWP
using Windows.Networking.Sockets;
#endif

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Class managing a remote client connection to a local <see cref="HttpServer"/>.
	/// </summary>
	internal class HttpClientConnection : CommunicationLayer, IDisposableAsync
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
		private WebSocket webSocket = null;
		private Encoding rxEncoding = null;
		private HTTP2.BinaryReader reader = null;
		private byte b1 = 0;
		private byte b2 = 0;
		private byte b3 = 0;
		private readonly bool encrypted;
		private readonly int port;
		private bool disposed = false;
		private bool rxText = false;

		// HTTP/2
		private int http2State = 0;
		private int http2FrameLength = 0;
		private int http2StreamId = 0;
		private int http2LastCreatedStreamId = 0;
		private int http2LastPermittedStreamId = int.MaxValue;
		private int http2FramePos = 0;
		private int http2BuildingHeadersOnStream = 0;
		private int http2InitialMaxFrameSize = int.MaxValue;
		private FrameType http2FrameType = 0;
		private ConnectionSettings localSettings = null;
		private ConnectionSettings remoteSettings = null;
		private IFlowControl flowControl = null;
		private HeaderReader http2HeaderReader = null;
		private HeaderWriter http2HeaderWriter = null;
		private Dictionary<int, PrioritySettings> rfc9218PrioritySettings = null;
		private byte http2FrameFlags = 0;
		private byte[] http2Frame = null;

		internal ConnectionSettings LocalSettings => this.localSettings;
		internal ConnectionSettings RemoteSettings => this.remoteSettings;
		internal HeaderReader HttpHeaderReader => this.http2HeaderReader;
		internal HeaderWriter HttpHeaderWriter => this.http2HeaderWriter;

		internal HttpClientConnection(HttpServer Server, BinaryTcpClient Client,
			bool Encrypted, int Port, params ISniffer[] Sniffers)
			: base(false, Sniffers)
		{
			this.server = Server;
			this.client = Client;
			this.encrypted = Encrypted;
			this.port = Port;

			this.client.OnDisconnected += this.Client_OnDisconnected;
			this.client.OnError += this.Client_OnError;
			this.client.OnReceived += this.Client_OnReceivedHttp1;
		}

		private Task<bool> Client_OnReceivedHttp1(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			this.server.DataReceived(Count);

			if (this.header is null)
				return this.BinaryHeader1Received(ConstantBuffer, Buffer, Offset, Count);
			else
				return this.BinaryData1Received(ConstantBuffer, Buffer, Offset, Count);
		}

		private async Task<bool> BinaryHeader1Received(bool ConstantBuffer, byte[] Data, int Offset, int NrRead)
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
					this.ReceiveText(Header);

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

				if (this.header.HttpVersion < 0)
				{
					await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Invalid connection preface");
					return false;
				}
				else if (this.header.HttpVersion < 1)
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
					this.client.OnReceivedReset(this.Client_OnReceivedHttp2Init);

					this.localSettings = new ConnectionSettings(
						this.server.Http2InitialWindowSize,
						this.server.Http2MaxFrameSize,
						this.server.Http2MaxConcurrentStreams,
						this.server.Http2HeaderTableSize,
						this.server.Http2EnablePush,
						this.server.Http2NoRfc7540Priorities);

					if (i + 1 < NrRead)
						return await this.Client_OnReceivedHttp2Init(null, ConstantBuffer, Data, i + 1, NrRead - i - 1);
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
							ConnectionSettings Settings = null;

							if (ConnectionSettings.TryParse(true, Bin, ref Settings))
							{
								this.remoteSettings = Settings;
								this.http2InitialMaxFrameSize = this.remoteSettings.MaxFrameSize;
								this.localSettings = new ConnectionSettings(
									this.server.Http2InitialWindowSize,
									this.server.Http2MaxFrameSize,
									this.server.Http2MaxConcurrentStreams,
									this.server.Http2HeaderTableSize,
									this.server.Http2EnablePush && this.remoteSettings.EnablePush,
									this.server.Http2NoRfc7540Priorities || this.remoteSettings.NoRfc7540Priorities);

								this.client.OnReceivedReset(this.Client_OnReceivedHttp2Live);

								if (this.localSettings.NoRfc7540Priorities)
									this.flowControl = new FlowControlRfc9218(this.localSettings, this.remoteSettings);
								else
									this.flowControl = new FlowControlRfc7540(this.localSettings, this.remoteSettings);

								using (HttpResponse Response = new HttpResponse(this.client, this, this.server, null)
								{
									StatusCode = 101,
									StatusMessage = "Switching Protocols",
									ContentLength = null,
									ContentType = null,
									ContentLanguage = null
								})
								{
									Response.SetHeader("Upgrade", "h2c");
									Response.SetHeader("Connection", "Upgrade");

									await Response.SendResponse();
								}

								if (i + 1 < NrRead)
									return await this.Client_OnReceivedHttp2Live(null, ConstantBuffer, Data, i + 1, NrRead - i - 1);
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
						return await this.BinaryData1Received(ConstantBuffer, Data, i + 1, NrRead - i - 1);

					if (!this.header.HasMessageBody)
						return await this.RequestReceived();

					return true;
				}
			}

			if (this.headerStream is null)
				this.headerStream = new MemoryStream();

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
					this.ReceiveBinary(true, Data2);
				}

				await this.SendResponse(null, null, new HttpException(431, "Request Header Fields Too Large",
					"Max Header Size: " + MaxHeaderSize.ToString()), true);
				return false;
			}
		}

		private async Task<bool> BinaryData1Received(bool ConstantBuffer, byte[] Data, int Offset, int NrRead)
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

			ulong DecodingResponse = await this.transferEncoding.DecodeAsync(ConstantBuffer, Data, Offset, NrRead);
			int NrAccepted = (int)DecodingResponse;
			bool Complete = (DecodingResponse & 0x100000000) != 0;

			if (this.HasSniffers)
			{
				if (Offset == 0 && NrAccepted == Data.Length)
				{
					if (this.rxText)
						this.ReceiveText(this.rxEncoding.GetString(Data));
					else
						this.ReceiveBinary(ConstantBuffer, Data);
				}
				else
				{
					if (this.rxText)
						this.ReceiveText(this.rxEncoding.GetString(Data, Offset, NrAccepted));
					else
					{
						byte[] Data2 = new byte[NrAccepted];
						Array.Copy(Data, Offset, Data2, 0, NrAccepted);
						this.ReceiveBinary(true, Data2);
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
						return await this.BinaryHeader1Received(ConstantBuffer, Data, Offset, NrRead);
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

		private async Task<bool> Client_OnReceivedHttp2Init(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			if (!(Sender is null))
				this.server.DataReceived(Count);

			int i, c;
			byte b;

			c = Offset + Count;

			for (i = Offset; i < c; i++)
			{
				b = Buffer[i];

				if (b != http2Preface[this.localSettings.InitStep++])
				{
					if (this.HasSniffers && i > Offset)
						this.ReceiveText(InternetContent.ISO_8859_1.GetString(Buffer, Offset, i - Offset));

					await this.SendResponse(null, null, new HttpException(405, "Method Not Allowed", "Invalid HTTP/2 connection preface."), true);
					return false;
				}

				if (this.localSettings.InitStep == 6)
				{
					this.client.OnReceivedReset(this.Client_OnReceivedHttp2Live);

					if (this.HasSniffers)
						this.ReceiveText("\r\nSM\r\n");

					if (i + 1 < Count)
						return await this.Client_OnReceivedHttp2Live(null, ConstantBuffer, Buffer, i + 1, c - i - 1);
					else
					{
						if (!this.localSettings.AcknowledgedOrSent)
						{
							this.localSettings.AcknowledgedOrSent = true;

							StringBuilder sb = this.HasSniffers ? new StringBuilder() : null;

							if (!await this.SendHttp2Frame(FrameType.Settings, 0, false, 0, null, this.localSettings.ToArray(sb)))
								return false;

							if (!(sb is null))
								this.TransmitText(sb.ToString().Trim());
						}

						return true;
					}
				}
			}

			return true;
		}

		private static readonly char[] http2Preface = new char[] { 'S', 'M', '\r', '\n', '\r', '\n' };

		private async Task<bool> Client_OnReceivedHttp2Live(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			if (!(Sender is null))
				this.server.DataReceived(Count);

			if (this.HasSniffers)
				this.ReceiveBinary(ConstantBuffer, Buffer, Offset, Count);

			int End = Offset + Count;
			int i;
			bool FramesProcessed = false;

			while (Offset < End)
			{
				switch (this.http2State)
				{
					case 0: // Frame length MSB
						if (End - Offset >= 9)  // Entire header in buffer
						{
							this.http2FrameLength = Buffer[Offset++];
							this.http2FrameLength <<= 8;
							this.http2FrameLength |= Buffer[Offset++];
							this.http2FrameLength <<= 8;
							this.http2FrameLength |= Buffer[Offset++];

							this.http2FrameType = (FrameType)Buffer[Offset++];
							this.http2FrameFlags = Buffer[Offset++];

							this.http2StreamId = Buffer[Offset++] & 127;
							this.http2StreamId <<= 8;
							this.http2StreamId |= Buffer[Offset++];
							this.http2StreamId <<= 8;
							this.http2StreamId |= Buffer[Offset++];
							this.http2StreamId <<= 8;
							this.http2StreamId |= Buffer[Offset++];

							if (this.http2FrameLength == 0)
							{
								if (!await this.ProcessHttp2Frame(true, emptyFrame, 0, 0))
									return false;

								FramesProcessed = true;
							}
							else
							{
								if (this.http2FrameLength > this.localSettings.MaxFrameSize)
									this.http2State = 10;
								else
								{
									i = End - Offset;
									if (i > 0)
									{
										i = Math.Min(i, this.http2FrameLength);

										if (i >= this.http2FrameLength)
										{
											if (!await this.ProcessHttp2Frame(ConstantBuffer, Buffer, Offset, i))
												return false;

											Offset += i;
											FramesProcessed = true;
										}
										else
										{
											if (this.http2Frame is null)
												this.http2Frame = new byte[this.localSettings.MaxFrameSize];

											Array.Copy(Buffer, Offset, this.http2Frame, 0, i);
											Offset += i;
											this.http2FramePos = i;
											this.http2State = 9;
										}
									}
									else
									{
										if (this.http2Frame is null)
											this.http2Frame = new byte[this.localSettings.MaxFrameSize];

										this.http2State = 9;
										this.http2FramePos = 0;
									}
								}
							}
						}
						else
						{
							this.http2FrameLength = Buffer[Offset++];
							this.http2State++;
						}
						break;

					case 1: // Frame length, bytes 2 & 3
					case 2:
						this.http2FrameLength <<= 8;
						this.http2FrameLength |= Buffer[Offset++];
						this.http2State++;
						break;

					case 3: // Frame type
						this.http2FrameType = (FrameType)Buffer[Offset++];
						this.http2State++;
						break;

					case 4: // Frame flags
						this.http2FrameFlags = Buffer[Offset++];
						this.http2State++;
						break;

					case 5: // Stream ID, MSB
						this.http2StreamId = Buffer[Offset++] & 127;
						this.http2State++;
						break;

					case 6: // Stream ID, bytes 2 & 3
					case 7:
						this.http2StreamId <<= 8;
						this.http2StreamId |= Buffer[Offset++];
						this.http2State++;
						break;

					case 8: // Stream ID, byte 4
						this.http2StreamId <<= 8;
						this.http2StreamId |= Buffer[Offset++];

						if (this.http2FrameLength == 0)
						{
							this.http2State = 0;
							if (!await this.ProcessHttp2Frame(true, emptyFrame, 0, 0))
								return false;

							FramesProcessed = true;
						}
						else
						{
							if (this.http2FrameLength > this.localSettings.MaxFrameSize)
								this.http2State += 2;
							else
							{
								i = End - Offset;
								if (i > 0)
								{
									i = Math.Min(i, this.http2FrameLength);

									if (i >= this.http2FrameLength)
									{
										if (!await this.ProcessHttp2Frame(ConstantBuffer, Buffer, Offset, i))
											return false;

										Offset += i;
										FramesProcessed = true;
										this.http2State = 0;
									}
									else
									{
										if (this.http2Frame is null)
											this.http2Frame = new byte[this.localSettings.MaxFrameSize];

										Array.Copy(Buffer, Offset, this.http2Frame, 0, i);
										Offset += i;
										this.http2FramePos = i;
										this.http2State++;
									}
								}
								else
								{
									if (this.http2Frame is null)
										this.http2Frame = new byte[this.localSettings.MaxFrameSize];

									this.http2State++;
									this.http2FramePos = 0;
								}
							}
						}
						break;

					case 9: // Frame payload
						i = Math.Min(End - Offset, this.http2FrameLength - this.http2FramePos);
						Array.Copy(Buffer, Offset, this.http2Frame, this.http2FramePos, i);
						Offset += i;
						this.http2FramePos += i;

						if (this.http2FramePos >= this.http2FrameLength)
						{
							this.http2State = 0;

							if (!await this.ProcessHttp2Frame(false, this.http2Frame, 0, this.http2FramePos))
								return false;

							FramesProcessed = true;
						}
						break;

					case 10:    // Skip Frame payload and return error.
						i = Math.Min(End - Offset, this.http2FrameLength - this.http2FramePos);
						Offset += i;
						this.http2FramePos += i;

						if (this.http2FramePos >= this.http2FrameLength)
						{
							this.http2State = 0;

							if (!await this.ReturnHttp2Error(Http2Error.FrameSizeError, 0, "Frame too large."))
								return false;
						}
						break;

					default:
						return false;
				}
			}

			if (FramesProcessed)
				return await this.SendPendingWindowUpdates();
			else
				return true;
		}

		private Task<bool> Client_OnReceivedWebSocket(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			this.server.DataReceived(Count);

			return this.webSocket?.WebSocketDataReceived(ConstantBuffer, Buffer, Offset, Count) ?? Task.FromResult(false);
		}

		private Task<bool> Client_OnReceivedClosed(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			this.server.DataReceived(Count);

			return Task.FromResult(false);
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
		public void Dispose()
		{
			this.DisposeAsync().Wait();
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

				this.flowControl?.Dispose();
				this.flowControl = null;
			}
		}

		internal Guid Id => this.id;
		internal HttpServer Server => this.server;
		internal bool Disposed => this.disposed;
		internal BinaryTcpClient Client => this.client;
		internal bool Encrypted => this.encrypted;
		internal int Port => this.port;

		private static void AppendFlags(FrameType Type, byte Flags, StringBuilder sb)
		{
			switch (Type)
			{
				case FrameType.Data:
					if ((Flags & 1) != 0)
						sb.Append(", End Stream");

					if ((Flags & 8) != 0)
						sb.Append(", Padded");
					break;

				case FrameType.Headers:
				case FrameType.Continuation:
					if ((Flags & 1) != 0)
						sb.Append(", End Stream");

					if ((Flags & 4) != 0)
						sb.Append(", End Headers");

					if ((Flags & 8) != 0)
						sb.Append(", Padded");

					if ((Flags & 32) != 0)
						sb.Append(", Priority");
					break;

				case FrameType.ResetStream:
					break;

				case FrameType.Settings:
					if ((Flags & 1) != 0)
						sb.Append(", ACK");
					break;

				case FrameType.Ping:
					if ((Flags & 1) != 0)
						sb.Append(", ACK");
					break;

				case FrameType.Priority:
				case FrameType.WindowUpdate:
				case FrameType.PriorityUpdate:
				case FrameType.GoAway:
					// No flags.
					break;

				case FrameType.PushPromise:
				// TODO: process frame and return response.
				default:
					sb.Append(", Flags ");
					sb.Append(Flags.ToString("X2"));
					break;
			}
		}

		private static readonly byte[] emptyFrame = Array.Empty<byte>();

		private async Task<bool> ProcessHttp2Frame(bool ConstantBuffer, byte[] FramePayload, int Offset, int Count)
		{
			try
			{
#if INFO_IN_SNIFFERS
				StringBuilder sb = null;
#else
				StringBuilder sb;
#endif

#if INFO_IN_SNIFFERS
				if (this.HasSniffers)
				{
					sb = new StringBuilder();

					if (this.http2StreamId > this.http2LastPermittedStreamId)
						sb.Append("Ignoring: ");
					else
						sb.Append("RX: ");

					sb.Append(this.http2FrameType.ToString());
					AppendFlags(this.http2FrameType, this.http2FrameFlags, sb);
					sb.Append(", Stream ");
					sb.Append(this.http2StreamId.ToString());
					sb.Append(", Size ");
					sb.Append(this.http2FrameLength.ToString());
					sb.Append(" bytes");

					if (this.http2FrameType != FrameType.Data && this.http2FrameType != FrameType.WindowUpdate)
						this.Information(sb.ToString());
				}
#endif
				if (this.http2StreamId > this.http2LastPermittedStreamId)
					return true;

				if (this.reader is null)
					this.reader = new HTTP2.BinaryReader(ConstantBuffer, FramePayload, Offset, Count);
				else
					this.reader.Reset(ConstantBuffer, FramePayload, Offset, Count);

				if (this.http2BuildingHeadersOnStream > 0 && this.http2FrameType != FrameType.Continuation)
					return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Expected continuation frame.");

				switch (this.http2FrameType)
				{
					case FrameType.Data:
						if (this.http2StreamId == 0)
						{
#if INFO_IN_SNIFFERS
							if (this.HasSniffers)
								this.Information(sb.ToString());
#endif
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream is 0");
						}

						if (!this.flowControl.TryGetStream(this.http2StreamId, out Http2Stream Stream))
						{
#if INFO_IN_SNIFFERS
							if (this.HasSniffers)
								this.Information(sb.ToString());
#endif
							if (this.http2StreamId <= this.http2LastCreatedStreamId)
								return await this.ReturnHttp2Error(Http2Error.StreamClosed, this.http2StreamId, "Stream no longer under flow control.");
							else
								return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream not under flow control.");
						}

						if (Stream.State != StreamState.Open)
						{
#if INFO_IN_SNIFFERS
							if (this.HasSniffers)
								this.Information(sb.ToString());
#endif
							return await this.ReturnHttp2Error(Http2Error.StreamClosed, this.http2StreamId, "Stream state is " + Stream.State.ToString());
						}

						bool EndStream = (this.http2FrameFlags & 1) != 0;
						bool Padded = (this.http2FrameFlags & 8) != 0;
						byte PaddingLen = Padded ? this.reader.NextByte() : (byte)0;

						int DataSize = this.reader.BytesLeft - PaddingLen;
						if (DataSize < 0)
						{
#if INFO_IN_SNIFFERS
							if (this.HasSniffers)
								this.Information(sb.ToString());
#endif
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Padding length larger than available data.");
						}

#if INFO_IN_SNIFFERS
						if (this.HasSniffers)
						{
							sb.Append(" (total received: ");
							sb.Append((Stream.DataBytesReceived + DataSize).ToString());
							sb.Append(')');

							this.Information(sb.ToString());
						}
#endif
						if (DataSize > 0)
						{
							if (!await Stream.DataReceived(this.reader.ConstantBuffer, this.reader.Buffer, this.reader.Position, DataSize))
								return await this.ReturnHttp2Error(Http2Error.EnhanceYourCalm, this.http2StreamId, "Not sufficient resources available in stream.");

							if (Stream.DataBytesReceived > (Stream.ContentLength ?? long.MaxValue))
								return await this.ReturnHttp2Error(Http2Error.ProtocolError, this.http2StreamId, "Data exceeds Content-Length.");

							this.flowControl.AddPendingIncrement(Stream, DataSize);
						}

						if (EndStream)
						{
							Stream.State = StreamState.HalfClosedRemote;

							if (this.http2HeaderWriter is null)
							{
								this.http2HeaderWriter = new HeaderWriter(this.localSettings.HeaderTableSize,
									this.localSettings.HeaderTableSize);
							}

							if (!await this.RequestReceived(Stream.Headers ?? new HttpRequestHeader(2.0),
								Stream.InputDataStream, Stream))
							{
								return false;
							}
						}
						break;

					case FrameType.Headers:
					case FrameType.Continuation:
						if (this.http2StreamId == 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream is 0");

						if ((this.http2StreamId & 1) == 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Client can only use odd numbered stream IDs.");

						bool StreamCreated = false;
						bool ResetHeader = true;

						if (this.http2FrameType == FrameType.Headers)
						{
							if (this.http2BuildingHeadersOnStream != 0)
								return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Headers for stream " + this.http2BuildingHeadersOnStream.ToString() + " not ended.");

							if (!this.flowControl.TryGetStream(this.http2StreamId, out Stream))
							{
								if (this.http2StreamId < this.http2LastCreatedStreamId)
									return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream no longer under flow control.");
								else if (this.http2StreamId == this.http2LastCreatedStreamId)
									return await this.ReturnHttp2Error(Http2Error.StreamClosed, 0, "Stream no longer under flow control.");

								Stream = new Http2Stream(this.http2StreamId, this);
								StreamCreated = true;
								this.http2BuildingHeadersOnStream = this.http2StreamId;
							}
						}
						else
						{
							if (this.http2StreamId != this.http2BuildingHeadersOnStream)
								return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Building headers on stream " + this.http2BuildingHeadersOnStream.ToString());

							if (!this.flowControl.TryGetStream(this.http2StreamId, out Stream))
								return await this.ReturnHttp2Error(Http2Error.StreamClosed, this.http2StreamId, "Stream not under flow control.");
						}

						if (Stream.State == StreamState.Idle)
							Stream.State = StreamState.Open;
						else if (Stream.State == StreamState.Open)
							ResetHeader = false;
						else if (Stream.State != StreamState.HalfClosedRemote || this.http2FrameType != FrameType.Continuation)
							return await this.ReturnHttp2Error(Http2Error.StreamClosed, this.http2StreamId, "Stream state: " + Stream.State.ToString());

						string s;
						bool EndHeaders = (this.http2FrameFlags & 4) != 0;
						uint StreamIdDependency = 0;    // Root
						byte Weight = 16;               // Default weight, §5.3.5 RFC 7540
						bool Priority = false;
						bool Exclusive = false;

						EndStream = (this.http2FrameFlags & 1) != 0;

						if (this.http2FrameType == FrameType.Headers)
						{
							Priority = (this.http2FrameFlags & 32) != 0;
							Padded = (this.http2FrameFlags & 8) != 0;
							PaddingLen = Padded ? this.reader.NextByte() : (byte)0;

							if (Priority)
							{
								StreamIdDependency = this.reader.NextUInt32();

								Exclusive = (StreamIdDependency & 0x80000000) != 0;
								StreamIdDependency &= 0x7fffffff;
								Weight = this.reader.NextByte();
#if INFO_IN_SNIFFERS
								if (this.HasSniffers)
								{
									sb.Clear();
									sb.Append("Stream ");
									sb.Append(this.http2StreamId.ToString());
									sb.Append(" priority: Dependency: ");
									sb.Append(StreamIdDependency.ToString());
									sb.Append(", Exclusive: ");
									sb.Append(Exclusive.ToString());
									sb.Append(", Weight: ");
									sb.Append(Weight.ToString());

									this.Information(sb.ToString());
								}
#endif
								if (StreamIdDependency == this.http2StreamId)
									return await this.ReturnHttp2Error(Http2Error.ProtocolError, this.http2StreamId, "Dependency unto itself prohibited.");
							}
						}
						else
							PaddingLen = 0;

						int HeaderSize = this.reader.BytesLeft - PaddingLen;
						if (HeaderSize < 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Padding length larger than available data in frame.");

						if (StreamCreated)
						{
							if (!(this.rfc9218PrioritySettings is null) &&
								this.rfc9218PrioritySettings.TryGetValue(this.http2StreamId, out PrioritySettings PrioritySettings))
							{
								this.rfc9218PrioritySettings.Remove(this.http2StreamId);

								if (PrioritySettings.Rfc9218Priority.HasValue)
									Stream.Rfc9218Priority = PrioritySettings.Rfc9218Priority.Value;

								if (PrioritySettings.Rfc9218Incremental.HasValue)
									Stream.Rfc9218Incremental = PrioritySettings.Rfc9218Incremental.Value;
							}

							int OutputWindow = this.flowControl.AddStream(Stream, Weight, (int)StreamIdDependency, Exclusive);
							if (OutputWindow < 0)
								return await this.ReturnHttp2Error(Http2Error.RefusedStream, this.http2StreamId, "Too many streams open.");

							this.http2LastCreatedStreamId = this.http2StreamId;

#if INFO_IN_SNIFFERS
							if (this.HasSniffers)
							{
								sb.Clear();
								sb.Append("Stream ");
								sb.Append(this.http2StreamId);
								sb.Append(" created. (Window input/output size: ");
								sb.Append(Stream.DataInputWindowSize.ToString());
								sb.Append('/');
								sb.Append(OutputWindow.ToString());
								sb.Append(')');

								this.Information(sb.ToString());
							}
#endif
						}
						else
						{
							if (Priority && this.flowControl is FlowControlRfc7540 FlowControlRfc7540_2)
								FlowControlRfc7540_2.UpdatePriority(Stream, Weight, (int)StreamIdDependency, Exclusive);

							if (EndHeaders && !EndStream && this.http2FrameType == FrameType.Headers)
								return await this.ReturnHttp2Error(Http2Error.ProtocolError, this.http2StreamId, "Second headers frame on stream lacks END_STREAM.");
						}

						if (EndHeaders)
						{
							byte[] Buf;
							int Start;
							int Count2;

							this.http2BuildingHeadersOnStream = 0;

							if (Stream.IsBuildingHeaders)
							{
								if (!Stream.BuildHeaders(this.reader.Buffer, this.reader.Position, HeaderSize))
									return await this.ReturnHttp2Error(Http2Error.EnhanceYourCalm, this.http2StreamId, "Headers too large.");

								Buf = Stream.FinishBuildingHeaders();
								Start = 0;
								Count2 = Buf.Length;
							}
							else
							{
								Buf = this.reader.Buffer;
								Start = this.reader.Position;
								Count2 = HeaderSize;
							}

							if (this.http2HeaderReader is null)
							{
								this.http2HeaderReader = new HeaderReader(Buf, Start, Count2,
									this.localSettings.HeaderTableSize, this.localSettings.HeaderTableSize);
								ResetHeader = false;
							}
							else
								this.http2HeaderReader.Reset(Buf, Start, Count2);
#if INFO_IN_SNIFFERS
							sb?.Clear();
#else
							sb = new StringBuilder();
#endif
							if (!await this.http2HeaderReader.TryLock(10000))
								return await this.ReturnHttp2Error(Http2Error.InternalError, 0, "Unable to get access to HTTP/2 header reader.");

							try
							{
								if (ResetHeader)
									this.http2HeaderReader.Reset(Buf, Start, Count2);

								string Cookie = null;   // Ref: §8.1.2.5, RFC 7540

								while (this.http2HeaderReader.HasMore)
								{
									if (!this.http2HeaderReader.ReadHeader(out string HeaderName, out string HeaderValue, out _))
										return await this.ReturnHttp2Error(Http2Error.CompressionError, 0, "Unable to decode headers.");

									if (this.HasSniffers)
									{
										sb.Append(HeaderName);
										sb.Append(": ");
										sb.AppendLine(HeaderValue);
									}

									if (HeaderName == "cookie")
									{
										if (Cookie is null)
											Cookie = HeaderValue;
										else
											Cookie += "; " + HeaderValue;
									}
									else
									{
										s = Stream.AddParsedHeader(HeaderName, HeaderValue);
										if (!(s is null))
											return await this.ReturnHttp2Error(Http2Error.ProtocolError, this.http2StreamId, s);
									}
								}

								if (!string.IsNullOrEmpty(Cookie))
								{
									s = Stream.AddParsedHeader("cookie", Cookie);
									if (!(s is null))
										return await this.ReturnHttp2Error(Http2Error.ProtocolError, this.http2StreamId, s);
								}
							}
							finally
							{
								this.http2HeaderReader.Release();
							}

							if (this.HasSniffers)
								this.ReceiveText(sb.ToString());
						}
						else if (!Stream.BuildHeaders(this.reader.Buffer, this.reader.Position, HeaderSize))
							return await this.ReturnHttp2Error(Http2Error.EnhanceYourCalm, this.http2StreamId, "Headers too large.");

						if (EndHeaders)
						{
							if (string.IsNullOrEmpty(Stream.Headers?.Method))
								return await this.ReturnHttp2Error(Http2Error.ProtocolError, this.http2StreamId, "Missing method.");

							if (string.IsNullOrEmpty(Stream.Headers.UriScheme))
								return await this.ReturnHttp2Error(Http2Error.ProtocolError, this.http2StreamId, "Missing scheme.");

							if (string.IsNullOrEmpty(Stream.Headers.ResourcePart))
								return await this.ReturnHttp2Error(Http2Error.ProtocolError, this.http2StreamId, "Missing path.");
						}

						if (EndStream)
						{
							Stream.State = StreamState.HalfClosedRemote;

							if (this.http2HeaderWriter is null)
							{
								this.http2HeaderWriter = new HeaderWriter(this.localSettings.HeaderTableSize,
									this.localSettings.HeaderTableSize);
							}

							if (EndHeaders)
							{
								if (!await this.RequestReceived(Stream.Headers, Stream.InputDataStream, Stream))
									return false;
							}
						}
						else if (this.http2FrameType == FrameType.Continuation)
						{
							if (EndHeaders)
							{
								if (!await this.RequestReceived(Stream.Headers, Stream.InputDataStream, Stream))
									return false;
							}
						}
						else if (EndHeaders && Stream.Headers.Method == "CONNECT")
						{
							if (Stream.Protocol != "websocket" ||
								!this.server.TryGetResource(Stream.Headers.Resource, out HttpResource Resource, out string SubPath) ||
								!(Resource is WebSocketListener WebSocketListener) ||
								(!string.IsNullOrEmpty(SubPath) && !Resource.HandlesSubPaths))
							{
								this.flowControl.RemoveStream(this.http2StreamId);
								return await this.ReturnHttp2Error(Http2Error.Cancel, this.http2StreamId, "Unrecognized protocol, or not a web socket resource.");
							}

							if (this.http2HeaderWriter is null)
							{
								this.http2HeaderWriter = new HeaderWriter(this.localSettings.HeaderTableSize,
									this.localSettings.HeaderTableSize);
							}

							if (!await this.RequestReceived(Stream.Headers ?? new HttpRequestHeader(2.0),
								Stream.InputDataStream, Stream))
							{
								return false;
							}
						}
						break;

					case FrameType.Priority:
						if (this.http2StreamId == 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream is 0");

						if (this.reader.BytesLeft != 5)
							return await this.ReturnHttp2Error(Http2Error.FrameSizeError, 0, "Expected exactly 5 bytes of data.");

						StreamIdDependency = this.reader.NextUInt32();
						Exclusive = (StreamIdDependency & 0x80000000) != 0;
						Weight = this.reader.NextByte();
						StreamIdDependency &= 0x7fffffff;

						if (StreamIdDependency == this.http2StreamId)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, this.http2StreamId, "Dependency unto itself prohibited.");

						if (this.flowControl is FlowControlRfc7540 FlowControlRfc7540 &&
							this.flowControl.TryGetStream(this.http2StreamId, out Stream))
						{
							if (Stream.Headers is null)
								return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Priority frame sent on stream where headers have not been completed.");

							FlowControlRfc7540.UpdatePriority(Stream, Weight, (int)StreamIdDependency, Exclusive);
						}
						break;

					case FrameType.ResetStream:
						if (this.http2StreamId == 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream is 0");

						if (this.reader.BytesLeft != 4)
							return await this.ReturnHttp2Error(Http2Error.FrameSizeError, 0, "Expected exactly 4 bytes of data.");

						Http2Error? Error = (Http2Error)this.reader.NextUInt32();

						s = "Client reset the stream.";

						if (this.reader.BytesLeft > 0)
							s += " (" + Encoding.UTF8.GetString(this.reader.NextBytes()) + ")";

						this.LogError(Error.Value, s, false);

						if (!this.flowControl.RemoveStream(this.http2StreamId))
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream not under flow control.");

						if (this.http2StreamId == this.http2BuildingHeadersOnStream)
							this.http2BuildingHeadersOnStream = 0;
						break;

					case FrameType.Settings:
						// SETTINGS, §6.5, RFC 7540: https://datatracker.ietf.org/doc/html/rfc7540#section-6.5

						if (this.http2StreamId != 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream is non-zero");

						if ((this.http2FrameFlags & 1) != 0)    // ACK
						{
							if (this.http2FrameLength > 0)
								return await this.ReturnHttp2Error(Http2Error.FrameSizeError, 0, "Expected no data in ACK frame.");

							if (!(this.remoteSettings is null))
								break;

#if INFO_IN_SNIFFERS
							sb?.Clear();
#else
							sb = new StringBuilder();
#endif
							this.remoteSettings = new ConnectionSettings();
							this.http2InitialMaxFrameSize = this.remoteSettings.MaxFrameSize;
						}
						else
						{
#if INFO_IN_SNIFFERS
							sb?.Clear();
#else
							sb = new StringBuilder();
#endif
							Error = ConnectionSettings.TryParse(this.reader, sb, ref this.remoteSettings);

							if (!(sb is null))
							{
								this.ReceiveText(sb.ToString().Trim());
								sb.Clear();
							}

							if (Error.HasValue)
								return await this.ReturnHttp2Error(Error.Value, 0, "Invalid settings received.");

							if (this.http2InitialMaxFrameSize == int.MaxValue)
								this.http2InitialMaxFrameSize = this.remoteSettings.MaxFrameSize;
							else if (this.remoteSettings.MaxFrameSize < this.http2InitialMaxFrameSize)
								return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Maximum frame size cannot be less than originally stated.");

							this.localSettings.EnablePush &= this.remoteSettings.EnablePush;
							this.localSettings.NoRfc7540Priorities |= this.remoteSettings.NoRfc7540Priorities;
						}

						if (this.flowControl is null)
						{
							if (this.localSettings.NoRfc7540Priorities)
								this.flowControl = new FlowControlRfc9218(this.localSettings, this.remoteSettings);
							else
								this.flowControl = new FlowControlRfc7540(this.localSettings, this.remoteSettings);
						}
						else
							this.flowControl.RemoteSettingsUpdated();

						if (this.localSettings.AcknowledgedOrSent)
						{
							if (!await this.SendHttp2Frame(FrameType.Settings, 1, false, 0, null))   // Ack
								return false;
						}
						else
						{
							this.localSettings.AcknowledgedOrSent = true;

							if (!await this.SendHttp2Frame(FrameType.Settings, 0, false, 0, null, this.localSettings.ToArray(sb)))
								return false;

							if (!(sb is null))
								this.TransmitText(sb.ToString().Trim());
						}
						break;

					case FrameType.PushPromise:
						return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Client not allowed to send push promises");

					case FrameType.Ping:
						if (this.http2StreamId != 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream is non-zero");

						if (this.reader.BytesLeft != 8)
							return await this.ReturnHttp2Error(Http2Error.FrameSizeError, 0, "Expected exactly 8 bytes of data.");

						if ((this.http2FrameFlags & 1) == 0)    // No ACK
						{
							byte[] Data = this.reader.NextBytes(8);
							await this.SendHttp2Frame(FrameType.Ping, 1, true, 0, null, Data);
						}
						break;

					case FrameType.GoAway:
						if (this.http2StreamId != 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream is non-zero");

						if (this.reader.BytesLeft < 8)
							return await this.ReturnHttp2Error(Http2Error.FrameSizeError, 0, "Expected at least 8 bytes data payload.");

						this.http2LastPermittedStreamId = (int)(this.reader.NextUInt32() & 0x7fffffff);
						Error = (Http2Error)this.reader.NextUInt32();

						s = "Client requested connection to close.";

						if (this.reader.BytesLeft > 0)
							s += " (" + Encoding.UTF8.GetString(this.reader.NextBytes()) + ")";

						this.LogError(Error.Value, s, true);

						this.flowControl?.GoingAway(this.http2LastPermittedStreamId);
						break;

					case FrameType.WindowUpdate:
						// WINDOW_UPDATE, §6.9, RFC 7540: https://datatracker.ietf.org/doc/html/rfc7540#section-6.9

						if (this.reader.BytesLeft != 4)
						{
#if INFO_IN_SNIFFERS
							if (this.HasSniffers)
								this.Information(sb.ToString());
#endif
							return await this.ReturnHttp2Error(Http2Error.FrameSizeError, 0, "Expected exactly 4 bytes of data.");
						}

						uint Increment = this.reader.NextUInt32() & 0x7fffffff;

						if (Increment == 0)
						{
#if INFO_IN_SNIFFERS
							if (this.HasSniffers)
								this.Information(sb.ToString());
#endif
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, this.http2StreamId, "Increment set to 0.");
						}

						if (this.http2StreamId == 0)
						{
#if INFO_IN_SNIFFERS
							if (this.HasSniffers)
							{
								sb.Append(", Connection Window +");
								sb.Append(Increment.ToString());

								this.Information(sb.ToString());
							}
#endif
							int NewSize = this.flowControl?.ReleaseConnectionResources((int)Increment) ?? -1;
							if (NewSize < 0)
							{
#if INFO_IN_SNIFFERS
								if (this.HasSniffers)
									this.Information(sb.ToString());
#endif
								return await this.ReturnHttp2Error(Http2Error.FlowControlError, 0, "Unable to release connection resources.");
							}
						}
						else
						{
#if INFO_IN_SNIFFERS
							if (this.HasSniffers)
							{
								sb.Append(", Stream Window +");
								sb.Append(Increment.ToString());

								this.Information(sb.ToString());
							}
#endif
							switch (this.flowControl?.ReleaseStreamResources(this.http2StreamId, (int)Increment) ?? -1)
							{
								case -1:
									return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream not under flow control.");

								case -2:
									return await this.ReturnHttp2Error(Http2Error.FlowControlError, this.http2StreamId, "Window size overflow.");
							}
						}
						break;

					case FrameType.PriorityUpdate:
						if (this.http2StreamId != 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, 0, "Stream is non-zero");

						while (this.reader.HasMore)
						{
							if (this.reader.BytesLeft < 4)
								return await this.ReturnHttp2Error(Http2Error.FrameSizeError, 0, "Expected at least 4 bytes of data payload.");

							StreamIdDependency = this.reader.NextUInt32();
							StreamIdDependency &= 0x7fffffff;

							s = this.reader.NextString(Encoding.ASCII);

							int? Rfc9218Priority = null;
							bool? Rfc9218Incremental = null;

							foreach (KeyValuePair<string, string> P in CommonTypes.ParseFieldValues(s))
							{
								switch (P.Key)
								{
									case "u":
										if (int.TryParse(P.Value, out int i) && i >= 0 && i <= 7)
											Rfc9218Priority = i;
										break;

									case "i":
										Rfc9218Incremental = true;
										break;
								}
							}

							if (this.flowControl is FlowControlRfc9218 FlowControlRfc9218)
							{
								if (this.flowControl.TryGetStream(this.http2StreamId, out Stream))
									FlowControlRfc9218.UpdatePriority(Stream, Rfc9218Priority, Rfc9218Incremental);
								else if (this.http2StreamId > this.http2LastCreatedStreamId)
								{
									if (this.rfc9218PrioritySettings is null)
										this.rfc9218PrioritySettings = new Dictionary<int, PrioritySettings>();

									this.rfc9218PrioritySettings[this.http2StreamId] = new PrioritySettings()
									{
										Rfc9218Priority = Rfc9218Priority,
										Rfc9218Incremental = Rfc9218Incremental
									};
								}
							}
							break;
						}
						break;

					default:
						return await this.SendHttp2Frame(FrameType.Ping, 1, false, 0, null, new byte[8]);
				}

				return true;
			}
			catch (Exception ex)
			{
				if (this.HasSniffers)
					this.Exception(ex);

				return await this.ReturnHttp2Error(Http2Error.InternalError, 0, ex.Message);
			}
		}

		private async Task<bool> SendPendingWindowUpdates()
		{
			if (this.flowControl is null || !this.flowControl.HasPendingIncrements)
				return true;

			MemoryStream Output = null;
			StringBuilder sb = null;
			int Total = 0;
			int i, j;

			foreach (PendingWindowIncrement Increment in this.flowControl.GetPendingIncrements())
			{
				if (Output is null)
				{
					Output = new MemoryStream();

					if (this.HasSniffers)
					{
						sb = new StringBuilder();
						sb.Append("TX: WindowUpdate");
					}
				}

				i = Increment.NrBytes;
				Total += i;

				if (!Increment.Stream.SetInputWindowSizeIncrement((uint)i))
					return false;

				j = Increment.Stream.StreamId;

				Output.WriteByte(0);                // 4 bytes payload
				Output.WriteByte(0);
				Output.WriteByte(4);
				Output.WriteByte((byte)FrameType.WindowUpdate);
				Output.WriteByte(0);                // Flags
				Output.WriteByte((byte)(j >> 24));  // Stream ID
				Output.WriteByte((byte)(j >> 16));
				Output.WriteByte((byte)(j >> 8));
				Output.WriteByte((byte)j);
				Output.WriteByte((byte)(i >> 24));  // Payload
				Output.WriteByte((byte)(i >> 16));
				Output.WriteByte((byte)(i >> 8));
				Output.WriteByte((byte)i);

				if (this.HasSniffers)
				{
					sb.Append(", Stream ");
					sb.Append(j);
					sb.Append(" +");
					sb.Append(i);
				}
			}

			if (Total == 0)
				return true;

			Output.WriteByte(0);                // 4 bytes payload
			Output.WriteByte(0);
			Output.WriteByte(4);
			Output.WriteByte((byte)FrameType.WindowUpdate);
			Output.WriteByte(0);                // Flags
			Output.WriteByte(0);                // Stream ID = 0, Connection
			Output.WriteByte(0);
			Output.WriteByte(0);
			Output.WriteByte(0);
			Output.WriteByte((byte)(Total >> 24));  // Payload
			Output.WriteByte((byte)(Total >> 16));
			Output.WriteByte((byte)(Total >> 8));
			Output.WriteByte((byte)Total);

			if (this.HasSniffers)
			{
				sb.Append(", Connection +");
				sb.Append(Total);
			}

			if (this.client is null)
				return false;

			byte[] Data = Output.ToArray();
			if (!await this.client.SendAsync(true, Data, true))
				return false;

			if (this.HasSniffers)
			{
#if INFO_IN_SNIFFERS
				this.Information(sb.ToString());
#endif
				this.TransmitBinary(true, Data);
			}

			return true;
		}

		private static bool IsError(Http2Error Error)
		{
			switch (Error)
			{
				case Http2Error.NoError:
				case Http2Error.StreamClosed:
					return false;

				default:
					return true;
			}
		}

		private void LogError(Http2Error ErrorCode, string Reason, bool ConnectionError)
		{
			if (this.HasSniffers)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(ErrorCode.ToString());
				sb.Append(": ");
				sb.Append(Reason);

				if (!ConnectionError)
				{
					sb.Append(" (Stream ");
					sb.Append(this.http2StreamId.ToString());
					sb.Append(')');
				}

				if (IsError(ErrorCode))
					this.Error(sb.ToString());
				else
					this.Information(sb.ToString());
			}
		}

		internal async Task<bool> ReturnHttp2Error(Http2Error ErrorCode, int StreamId, string Reason)
		{
			bool ConnectionError = StreamId == 0;

			this.LogError(ErrorCode, Reason, ConnectionError);

			int i = (int)ErrorCode;

			if (ConnectionError)
			{
				byte[] ReasonBin = Encoding.UTF8.GetBytes(Reason);
				int c = ReasonBin.Length;
				byte[] Payload = new byte[c + 8];

				Payload[0] = (byte)(this.http2LastCreatedStreamId >> 24);
				Payload[1] = (byte)(this.http2LastCreatedStreamId >> 16);
				Payload[2] = (byte)(this.http2LastCreatedStreamId >> 8);
				Payload[3] = (byte)this.http2LastCreatedStreamId;
				Payload[4] = (byte)(i >> 24);
				Payload[5] = (byte)(i >> 16);
				Payload[6] = (byte)(i >> 8);
				Payload[7] = (byte)i;
				Array.Copy(ReasonBin, 0, Payload, 8, c);

				await this.SendHttp2Frame(FrameType.GoAway, 0, false, 0, null, Payload);

				this.client?.OnReceivedReset(this.Client_OnReceivedClosed);

				this.CloseConnection();
				return false;
			}
			else
			{
				return await this.SendHttp2Frame(FrameType.ResetStream, 0, false, StreamId, null,
					(byte)(i >> 24),
					(byte)(i >> 16),
					(byte)(i >> 8),
					(byte)i);
			}
		}

		internal bool IsStreamOpen(int StreamId)
		{
			return this.flowControl?.TryGetStream(StreamId, out _) ?? false;
		}

		/// <summary>
		/// Tries to write DATA to the remote party. Flow control might restrict the number
		/// of bytes written.
		/// </summary>
		/// <param name="Stream">Stream sending data.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary data</param>
		/// <param name="Offset">Offset into buffer where data begins.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Last">If it is the last data to be written for this stream.</param>
		/// <param name="DataEncoding">Optional encoding, if data is text.</param>
		/// <returns>Number of bytes written. If negative, request failed.</returns>
		internal async Task<int> TryWriteData(Http2Stream Stream, bool ConstantBuffer, byte[] Data, int Offset, int Count, bool Last,
			Encoding DataEncoding)
		{
			int StreamId = Stream.StreamId;

			if (Count == 0)
			{
				if (Last)
				{
					if (!await this.SendHttp2Frame(FrameType.Data, 1, false, StreamId, Stream, Data, Offset, 0, DataEncoding))   // END_STREAM
						return -1;
				}

				return 0;
			}

			if (this.flowControl is null)
				return -1;

			int NrBytes = await this.flowControl.RequestResources(StreamId, Count);
			if (NrBytes <= 0)
				return NrBytes;
			else if (NrBytes < Count)
				Last = false;

			if (!await this.SendHttp2Frame(FrameType.Data, Last ? (byte)1 : (byte)0, false,
				StreamId, Stream, Data, Offset, NrBytes, DataEncoding))
			{
				return -1;
			}

			if (Last)
				this.flowControl?.RemoveStream(StreamId);

			return NrBytes;
		}

		internal Task<bool> SendHttp2Frame(FrameType Type, byte Flags, bool Priority,
			int StreamId, Http2Stream Stream, params byte[] Payload)
		{
			return this.SendHttp2Frame(Type, Flags, Priority, StreamId, Stream, Payload, 0, Payload.Length, null);
		}

		internal async Task<bool> SendHttp2Frame(FrameType Type, byte Flags, bool Priority,
			int StreamId, Http2Stream Stream, byte[] Payload, int Offset, int Count, Encoding DataEncoding)
		{
			if (Count > 0x00ffffff)
				return false;

			byte[] Data = new byte[9 + Count];

			Data[0] = (byte)(Count >> 16);
			Data[1] = (byte)(Count >> 8);
			Data[2] = (byte)Count;
			Data[3] = (byte)Type;
			Data[4] = Flags;
			Data[5] = (byte)(StreamId >> 24);
			Data[6] = (byte)(StreamId >> 16);
			Data[7] = (byte)(StreamId >> 8);
			Data[8] = (byte)StreamId;

			if (Count > 0)
				Array.Copy(Payload, Offset, Data, 9, Count);

			if (this.client is null)
				return false;

			if (!await this.client.SendAsync(true, Data, Priority))
				return false;

			if (this.HasSniffers)
			{
#if INFO_IN_SNIFFERS
				StringBuilder sb = new StringBuilder();

				sb.Append("TX: ");
				sb.Append(Type.ToString());
				AppendFlags(Type, Flags, sb);
				sb.Append(", Stream ");
				sb.Append(StreamId.ToString());
				sb.Append(", Size ");
				sb.Append(Count.ToString());
				sb.Append(" bytes");

				if (Type == FrameType.Data && !(Stream is null))
				{
					sb.Append(" (total transmitted: ");
					sb.Append((Stream.DataBytesTransmitted + Count).ToString());
					sb.Append(')');
				}

				this.Information(sb.ToString());
#endif

				if (DataEncoding is null)
					this.TransmitBinary(true, Data);
				else
				{
					string s = DataEncoding.GetString(Payload, Offset, Count);

					if (ContainsControlCharacters(s))
						this.TransmitBinary(true, Data);
					else
					{
						this.TransmitBinary(true, Data, 0, 9);
						this.TransmitText(s);
					}
				}
			}

			return true;
		}

		internal static bool IsSniffableTextType(string ContentType)
		{
			if (string.IsNullOrEmpty(ContentType))
				return false;

			ContentType = ContentType.ToLower();
			int j = ContentType.IndexOf('/');
			if (j < 0)
				return false;

			string s = ContentType.Substring(0, j);

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
					return ContentType == "multipart/form-data";

				default:
					return false;
			}
		}

		private async Task<bool> RequestReceived()
		{
			bool Result = await this.RequestReceived(this.header, this.dataStream, null);

			this.header = null;
			this.dataStream = null;
			this.transferEncoding = null;

			return Result;
		}

		private async Task<bool> RequestReceived(HttpRequestHeader Header, Stream DataStream, Http2Stream Http2Stream)
		{
#if WINDOWS_UWP
			StreamSocket UnderlyingSocket = this.client.Client;
			HttpRequest Request = new HttpRequest(this.server, Header, DataStream,
				UnderlyingSocket.Information.RemoteAddress.ToString() + ":" + UnderlyingSocket.Information.RemotePort,
				UnderlyingSocket.Information.LocalAddress.ToString() + ":" + UnderlyingSocket.Information.LocalPort,
				Http2Stream);
#else
			Socket UnderlyingSocket = this.client.Client.Client;
			HttpRequest Request = new HttpRequest(this.server, Header, DataStream,
				UnderlyingSocket.RemoteEndPoint.ToString(),
				UnderlyingSocket.LocalEndPoint.ToString(),
				Http2Stream);
#endif
			Request.clientConnection = this;

			bool? Queued = await this.QueueRequest(Request);

			if (Queued.HasValue)
			{
				if (!Queued.Value && !(DataStream is null))
					DataStream.Dispose();

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

				this.Exception(ex);

				await this.SendResponse(Request, null, new NotImplementedException(ex.Message), !Result);
			}
			catch (IOException ex)
			{
				this.Exception(ex);

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
				this.Exception(ex);

				await this.SendResponse(Request, null, new InternalServerErrorException(ex.Message), !Result);
			}
			catch (Exception ex)
			{
				Result = Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData;

				this.Exception(ex);

				await this.SendResponse(Request, null, new InternalServerErrorException(ex.Message), !Result);
			}

			Request.Dispose();
			return Result;
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
					Response = new HttpResponse(this.client, this, this.server, Request)
					{
						Progress = Request.Http2Stream
					};

					await Resource.Execute(this.server, Request, Response);
				}
			}
			catch (HttpException ex)
			{
				if (Response is null || !Response.HeaderSent)
				{
					try
					{
						await this.SendResponse(Request, Response, ex, false, ex.HeaderFields);
					}
					catch (Exception)
					{
						this.CloseConnection();
					}
				}
				else
					this.CloseConnection();
			}
			catch (System.NotImplementedException ex)
			{
				this.Exception(ex);

				if (Response is null || !Response.HeaderSent)
				{
					try
					{
						await this.SendResponse(Request, Response, new NotImplementedException(ex.Message), true);
					}
					catch (Exception)
					{
						this.CloseConnection();
					}
				}
				else
					this.CloseConnection();
			}
			catch (IOException ex)
			{
				this.Exception(ex);

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
						this.CloseConnection();
					}
				}
				else
					this.CloseConnection();
			}
			catch (XmlException ex)
			{
				ex = XML.AnnotateException(ex);
				this.Exception(ex);

				if (Response is null || !Response.HeaderSent)
				{
					try
					{
						await this.SendResponse(Request, Response, new InternalServerErrorException(ex.Message), true);
					}
					catch (Exception)
					{
						this.CloseConnection();
					}
				}
				else
					this.CloseConnection();
			}
			catch (Exception ex)
			{
				this.Exception(ex);

				if (Response is null || !Response.HeaderSent)
				{
					try
					{
						await this.SendResponse(Request, Response, new InternalServerErrorException(ex.Message), true);
					}
					catch (Exception)
					{
						this.CloseConnection();
					}
				}
				else
					this.CloseConnection();
			}
			finally
			{
				Request.Dispose();
			}
		}

		private void CloseConnection()
		{
			this.client?.DisposeWhenDone();
			this.client = null;
		}

		private async Task SendResponse(HttpRequest Request, HttpResponse Response, HttpException ex,
			bool CloseAfterTransmission, params KeyValuePair<string, string>[] HeaderFields)
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
			this.client.OnReceivedReset(this.Client_OnReceivedWebSocket);
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
