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
using Waher.Runtime.Console.Worker;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;
using Waher.Script.Functions.Logging;
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
		WebSocket,
		Closed
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

				case ConnectionMode.Closed:
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
					this.localSettings = new ConnectionSettings(
						this.server.Http2InitialWindowSize,
						this.server.Http2MaxFrameSize,
						this.server.Http2MaxConcurrentStreams,
						this.server.Http2HeaderTableSize,
						this.server.Http2EnablePush,
						this.server.Http2NoRfc7540Priorities);

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
								this.remoteSettings = Settings;
								this.localSettings = new ConnectionSettings(
									this.server.Http2InitialWindowSize,
									this.server.Http2MaxFrameSize,
									this.server.Http2MaxConcurrentStreams,
									this.server.Http2HeaderTableSize,
									this.server.Http2EnablePush,
									this.server.Http2NoRfc7540Priorities || this.remoteSettings.NoRfc7540Priorities);

								this.mode = ConnectionMode.Http2Live;

								if (this.localSettings.NoRfc7540Priorities)
									this.flowControl = new FlowControlRfc9218(this.remoteSettings);
								else
									this.flowControl = new FlowControlRfc7540(this.remoteSettings);

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

				if (b != http2Preface[this.localSettings.InitStep++])
				{
					if (this.HasSniffers && i > Offset)
						await this.ReceiveText(InternetContent.ISO_8859_1.GetString(Data, Offset, i - Offset));

					await this.SendResponse(null, null, new HttpException(405, "Method Not Allowed", "Invalid HTTP/2 connection preface."), true);
					return false;
				}

				if (this.localSettings.InitStep == 6)
				{
					this.mode = ConnectionMode.Http2Live;

					if (this.HasSniffers)
						await this.ReceiveText("\r\nSM\r\n");

					if (i + 1 < NrRead)
						return await this.BinaryHttp2LiveDataReceived(Data, i + 1, c - i - 1);
					else
						return true;
				}
			}

			return true;
		}

		private async Task<bool> BinaryHttp2LiveDataReceived(byte[] Data, int Offset, int NrRead)
		{
			if (this.HasSniffers)
				await this.ReceiveBinary(BinaryTcpClient.ToArray(Data, Offset, NrRead));

			int End = Offset + NrRead;
			bool FramesProcessed = false;

			while (Offset < End)
			{
				switch (this.http2State)
				{
					case 0: // Frame length MSB
						this.http2FrameLength = Data[Offset++];
						this.http2State++;
						break;

					case 1: // Frame length, bytes 2 & 3
					case 2:
						this.http2FrameLength <<= 8;
						this.http2FrameLength |= Data[Offset++];
						this.http2State++;
						break;

					case 3: // Frame type
						this.http2FrameType = (FrameType)Data[Offset++];
						this.http2State++;
						break;

					case 4: // Frame flags
						this.http2FrameFlags = Data[Offset++];
						this.http2State++;
						break;

					case 5: // Stream ID, MSB
						this.http2StreamId = Data[Offset++] & 127;
						this.http2State++;
						break;

					case 6: // Stream ID, bytes 2 & 3
					case 7:
						this.http2StreamId <<= 8;
						this.http2StreamId |= Data[Offset++];
						this.http2State++;
						break;

					case 8: // Stream ID, byte 4
						this.http2StreamId <<= 8;
						this.http2StreamId |= Data[Offset++];

						if (this.http2FrameLength == 0)
						{
							this.http2State = 0;
							if (!await this.ProcessHttp2Frame())
								return false;

							FramesProcessed = true;
						}
						else
						{
							this.http2FramePos = 0;

							if (this.http2FrameLength > this.localSettings.MaxFrameSize)
								this.http2State += 2;
							else
							{
								this.http2Frame ??= new byte[this.localSettings.MaxFrameSize];
								this.http2State++;
							}
						}
						break;

					case 9: // Frame payload
						int i = Math.Min(End - Offset, this.http2FrameLength - this.http2FramePos);
						Array.Copy(Data, Offset, this.http2Frame, this.http2FramePos, i);
						Offset += i;
						this.http2FramePos += i;

						if (this.http2FramePos >= this.http2FrameLength)
						{
							this.http2State = 0;

							if (!await this.ProcessHttp2Frame())
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

							if (!await this.ReturnHttp2Error(Http2Error.FrameSizeError, true))
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

		private async Task<bool> ProcessHttp2Frame()
		{
			try
			{
				StringBuilder sb;

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

					await this.Information(sb.ToString());
				}

				if (this.http2StreamId > this.http2LastPermittedStreamId)
					return true;

				if (this.reader is null)
					this.reader = new HTTP2.BinaryReader(this.http2Frame, 0, this.http2FrameLength);
				else
					this.reader.Reset(this.http2Frame, 0, this.http2FrameLength);

				switch (this.http2FrameType)
				{
					case FrameType.Data:
						if (this.http2StreamId == 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

						if (!this.flowControl.TryGetStream(this.http2StreamId, out Http2Stream Stream) ||
							Stream.State != StreamState.Open)
						{
							return await this.ReturnHttp2Error(Http2Error.StreamClosed, false);
						}

						bool EndStream = (this.http2FrameFlags & 1) != 0;
						bool Padded = (this.http2FrameFlags & 8) != 0;
						byte PaddingLen = Padded ? this.reader.NextByte() : (byte)0;

						int DataSize = this.reader.BytesLeft - PaddingLen;
						if (DataSize < 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

						if (DataSize > 0)
						{
							if (!await Stream.DataReceived(this.reader.Buffer, this.reader.Position, DataSize))
								return await this.ReturnHttp2Error(Http2Error.EnhanceYourCalm, false);

							this.localSettings.AddPendingIncrement(Stream, DataSize);
						}

						if (EndStream)
						{
							Stream.State = StreamState.HalfClosedRemote;

							this.http2HeaderWriter ??= new HeaderWriter(this.localSettings.HeaderTableSize,
								this.localSettings.MaxHeaderListSize);

							if (!await this.RequestReceived(Stream.Headers, Stream.InputDataStream, Stream))
								return false;
						}
						break;

					case FrameType.Headers:
					case FrameType.Continuation:
						if (this.http2StreamId == 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

						bool StreamCreated = false;

						if (this.http2FrameType == FrameType.Headers)
						{
							if (!this.flowControl.TryGetStream(this.http2StreamId, out Stream))
							{
								if (this.http2StreamId < this.http2LastCreatedStreamId)
									return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

								Stream = new Http2Stream(this.http2StreamId, this);
								StreamCreated = true;
							}
						}
						else if (!this.flowControl.TryGetStream(this.http2StreamId, out Stream))
							return await this.ReturnHttp2Error(Http2Error.StreamClosed, false);

						if (Stream.State == StreamState.Idle)
							Stream.State = StreamState.Open;
						else if (Stream.State == StreamState.Open)
						{
							if (this.http2FrameType != FrameType.Continuation)
								return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);
						}
						else
							return await this.ReturnHttp2Error(Http2Error.StreamClosed, false);

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
							}
						}
						else
							PaddingLen = 0;

						int HeaderSize = this.reader.BytesLeft - PaddingLen;
						if (HeaderSize < 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

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
								return await this.ReturnHttp2Error(Http2Error.RefusedStream, false);

							this.http2LastCreatedStreamId = this.http2StreamId;

							if (this.HasSniffers)
							{
								sb = new StringBuilder();
								sb.Append("Stream ");
								sb.Append(this.http2StreamId);
								sb.Append(" creted. (Window input/output size: ");
								sb.Append(Stream.DataInputWindowSize.ToString());
								sb.Append('/');
								sb.Append(OutputWindow.ToString());
								sb.Append(')');

								await this.Information(sb.ToString());
							}
						}
						else if (Priority && this.flowControl is FlowControlRfc7540 FlowControlRfc7540_2)
							FlowControlRfc7540_2.UpdatePriority(Stream, Weight, (int)StreamIdDependency, Exclusive);

						if (EndHeaders)
						{
							byte[] Buf;
							int Start;
							int Count;

							if (Stream.IsBuildingHeaders)
							{
								if (!Stream.BuildHeaders(this.reader.Buffer, this.reader.Position, HeaderSize))
									return await this.ReturnHttp2Error(Http2Error.EnhanceYourCalm, false);

								Buf = Stream.FinishBuildingHeaders();
								Start = 0;
								Count = Buf.Length;
							}
							else
							{
								Buf = this.reader.Buffer;
								Start = this.reader.Position;
								Count = HeaderSize;
							}

							bool ResetHeader = true;

							if (this.http2HeaderReader is null)
							{
								this.http2HeaderReader = new HeaderReader(Buf, Start, Count,
									this.localSettings.HeaderTableSize, this.localSettings.MaxHeaderListSize);
								ResetHeader = false;
							}

							await this.http2HeaderReader.Lock();
							try
							{
								if (ResetHeader)
									this.http2HeaderReader.Reset(Buf, Start, Count);

								string Cookie = null;   // Ref: §8.1.2.5, RFC 7540

								while (this.http2HeaderReader.HasMore)
								{
									if (!this.http2HeaderReader.ReadHeader(out string HeaderName, out string HeaderValue, out _))
										return false;

									if (this.HasSniffers)
										await this.ReceiveText(HeaderName + ": " + HeaderValue);

									if (HeaderName == "cookie")
									{
										if (Cookie is null)
											Cookie = HeaderValue;
										else
											Cookie += "; " + HeaderValue;
									}
									else
										Stream.AddParsedHeader(HeaderName, HeaderValue);
								}

								if (!string.IsNullOrEmpty(Cookie))
									Stream.AddParsedHeader("cookie", Cookie);
							}
							finally
							{
								this.http2HeaderReader.Release();
							}
						}
						else if (!Stream.BuildHeaders(this.reader.Buffer, this.reader.Position, HeaderSize))
							return await this.ReturnHttp2Error(Http2Error.EnhanceYourCalm, false);

						if (EndStream)
						{
							Stream.State = StreamState.HalfClosedRemote;

							this.http2HeaderWriter ??= new HeaderWriter(this.localSettings.HeaderTableSize,
								this.localSettings.MaxHeaderListSize);

							if (!await this.RequestReceived(Stream.Headers, Stream.InputDataStream, Stream))
								return false;
						}
						else if (EndHeaders && Stream.Headers.Method == "CONNECT")
						{
							if (!Stream.Headers.TryGetHeaderField(":protocol", out HttpField Protocol) ||
								Protocol.Value != "websocket" ||
								!this.server.TryGetResource(Stream.Headers.Resource, out HttpResource Resource, out string SubPath) ||
								!(Resource is WebSocketListener WebSocketListener) ||
								(!string.IsNullOrEmpty(SubPath) && !Resource.HandlesSubPaths))
							{
								this.flowControl.RemoveStream(this.http2StreamId);
								return await this.ReturnHttp2Error(Http2Error.Cancel, false);
							}

							this.http2HeaderWriter ??= new HeaderWriter(this.localSettings.HeaderTableSize,
								this.localSettings.MaxHeaderListSize);

							if (!await this.RequestReceived(Stream.Headers, Stream.InputDataStream, Stream))
								return false;
						}
						break;

					case FrameType.Priority:
						if (this.http2StreamId == 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

						if (this.reader.BytesLeft != 5)
							return await this.ReturnHttp2Error(Http2Error.FrameSizeError, true);

						StreamIdDependency = this.reader.NextUInt32();
						Exclusive = (StreamIdDependency & 0x80000000) != 0;
						Weight = this.reader.NextByte();
						StreamIdDependency &= 0x7fffffff;

						if (this.flowControl is FlowControlRfc7540 FlowControlRfc7540 &&
							this.flowControl.TryGetStream(this.http2StreamId, out Stream))
						{
							FlowControlRfc7540.UpdatePriority(Stream, Weight, (int)StreamIdDependency, Exclusive);
						}
						break;

					case FrameType.ResetStream:
						if (this.http2StreamId == 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

						if (this.reader.BytesLeft != 4)
							return await this.ReturnHttp2Error(Http2Error.FrameSizeError, true);

						Http2Error? Error = (Http2Error)this.reader.NextUInt32();
						await this.LogError(Error.Value);

						this.flowControl.RemoveStream(this.http2StreamId);
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
							sb = this.HasSniffers ? new StringBuilder() : null;
							Error = ConnectionSettings.TryParse(this.reader, sb, out this.remoteSettings);

							if (!(sb is null))
							{
								await this.ReceiveText(sb.ToString().Trim());
								sb.Clear();
							}

							if (Error.HasValue)
								return await this.ReturnHttp2Error(Error.Value, true);

							this.localSettings.NoRfc7540Priorities |= this.remoteSettings.NoRfc7540Priorities;

							if (this.flowControl is null)
							{
								if (this.localSettings.NoRfc7540Priorities)
									this.flowControl = new FlowControlRfc9218(this.remoteSettings);
								else
									this.flowControl = new FlowControlRfc7540(this.remoteSettings);
							}
							else
								this.flowControl.UpdateSettings(this.remoteSettings);

							if (this.localSettings.AcknowledgedOrSent)
							{
								if (!await this.SendHttp2Frame(FrameType.Settings, 1, false, null))   // Ack
									return false;
							}
							else
							{
								this.localSettings.AcknowledgedOrSent = true;

								if (!await this.SendHttp2Frame(FrameType.Settings, 0, false, null, this.localSettings.ToArray(sb)))
									return false;

								if (!(sb is null))
									await this.TransmitText(sb.ToString().Trim());
							}
						}
						break;

					case FrameType.PushPromise:
						// TODO: process frame and return response.
						break;

					case FrameType.Ping:
						if (this.http2StreamId != 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

						if (this.reader.BytesLeft != 8)
							return await this.ReturnHttp2Error(Http2Error.FrameSizeError, true);

						if ((this.http2FrameFlags & 1) == 0)    // No ACK
						{
							byte[] Data = this.reader.NextBytes(8);
							await this.SendHttp2Frame(FrameType.Ping, 1, true, null, Data);
						}
						break;

					case FrameType.GoAway:
						if (this.http2StreamId != 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

						if (this.reader.BytesLeft < 8)
							return await this.ReturnHttp2Error(Http2Error.FrameSizeError, true);

						this.http2LastPermittedStreamId = (int)(this.reader.NextUInt32() & 0x7fffffff);
						Error = (Http2Error)this.reader.NextUInt32();
						await this.LogError(Error.Value);

						this.flowControl?.GoingAway(this.http2LastPermittedStreamId);
						break;

					case FrameType.WindowUpdate:
						// WINDOW_UPDATE, §6.9, RFC 7540: https://datatracker.ietf.org/doc/html/rfc7540#section-6.9

						if (this.reader.BytesLeft != 4)
							return await this.ReturnHttp2Error(Http2Error.FrameSizeError, true);

						uint Increment = this.reader.NextUInt32() & 0x7fffffff;

						if (Increment == 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, this.http2StreamId == 0);

						int NewSize;

						if (this.http2StreamId == 0)
						{
							NewSize = this.flowControl?.ReleaseConnectionResources((int)Increment) ?? -1;
							if (NewSize < 0)
								return await this.ReturnHttp2Error(Http2Error.FlowControlError, true);

							if (this.HasSniffers)
							{
								sb = new StringBuilder();

								sb.Append("Connection Window +");
								sb.Append(Increment.ToString());
								sb.Append(" (Available for output: ");
								sb.Append(NewSize.ToString());
								sb.Append(')');

								await this.Information(sb.ToString());
							}
						}
						else
						{
							NewSize = this.flowControl?.ReleaseStreamResources(this.http2StreamId, (int)Increment) ?? -1;
							// Ignore returning error if stream has been removed.

							if (this.HasSniffers)
							{
								sb = new StringBuilder();

								sb.Append("Stream ");
								sb.Append(this.http2StreamId.ToString());
								sb.Append(" Window +");
								sb.Append(Increment.ToString());

								if (NewSize < 0)
									sb.Append(" (stream removed)");
								else
								{
									sb.Append(" (Available for output: ");
									sb.Append(NewSize.ToString());
									sb.Append(')');
								}

								await this.Information(sb.ToString());
							}
						}
						break;

					case FrameType.PriorityUpdate:
						if (this.http2StreamId != 0)
							return await this.ReturnHttp2Error(Http2Error.ProtocolError, true);

						while (this.reader.HasMore)
						{
							if (this.reader.BytesLeft < 4)
								return await this.ReturnHttp2Error(Http2Error.FrameSizeError, true);

							StreamIdDependency = this.reader.NextUInt32();
							StreamIdDependency &= 0x7fffffff;

							string s = this.reader.NextString(Encoding.ASCII);
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
									this.rfc9218PrioritySettings ??= new Dictionary<int, PrioritySettings>();
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
						return await this.ReturnHttp2Error(Http2Error.ProtocolError, false);
				}

				return true;
			}
			catch (Exception ex)
			{
				if (this.HasSniffers)
					await this.Exception(ex);

				return await this.ReturnHttp2Error(Http2Error.InternalError, true);
			}
		}

		private async Task<bool> SendPendingWindowUpdates()
		{
			if (!this.localSettings.HasPendingIncrements)
				return true;

			long Total = 0;
			int i;

			foreach (ConnectionSettings.PendingWindowIncrement Increment in this.localSettings.GetPendingIncrements())
			{
				i = Increment.NrBytes;
				Total += i;

				if (!Increment.Stream.SetInputWindowSizeIncrement((uint)i))
					return false;

				if (!await this.SendHttp2Frame(FrameType.WindowUpdate, 0, false, Increment.Stream,
					(byte)(i >> 24),
					(byte)(i >> 16),
					(byte)(i >> 8),
					(byte)i))
				{
					return false;
				}
			}

			if (!await this.SendHttp2Frame(FrameType.WindowUpdate, 0, false, null,
				(byte)(Total >> 24),
				(byte)(Total >> 16),
				(byte)(Total >> 8),
				(byte)Total))
			{
				return false;
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

		private Task LogError(Http2Error ErrorCode)
		{
			if (this.HasSniffers)
			{
				if (IsError(ErrorCode))
					return this.Error(ErrorCode.ToString());
				else
					return this.Information(ErrorCode.ToString());
			}
			else
				return Task.CompletedTask;
		}

		private async Task<bool> ReturnHttp2Error(Http2Error ErrorCode, bool ConnectionError)
		{
			await this.LogError(ErrorCode);

			int i = (int)ErrorCode;

			if (ConnectionError)
			{
				await this.SendHttp2Frame(FrameType.GoAway, 0, false, null,
					(byte)(this.http2LastCreatedStreamId >> 24),
					(byte)(this.http2LastCreatedStreamId >> 16),
					(byte)(this.http2LastCreatedStreamId >> 8),
					(byte)this.http2LastCreatedStreamId,
					(byte)(i >> 24),
					(byte)(i >> 16),
					(byte)(i >> 8),
					(byte)i);
				this.mode = ConnectionMode.Closed;

				return false;
			}
			else
			{
				return await this.SendHttp2Frame(FrameType.ResetStream, 0, false, null,
					(byte)(i >> 24),
					(byte)(i >> 16),
					(byte)(i >> 8),
					(byte)i);
			}
		}

		/// <summary>
		/// Writes DATA to the remote party.
		/// </summary>
		/// <param name="Stream">Stream sending data.</param>
		/// <param name="Data">Binary data</param>
		/// <param name="Offset">Offset into buffer where data begins.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Last">If it is the last data to be written for this stream.</param>
		/// <param name="DataEncoding">Optional encoding, if data is text.</param>
		/// <returns>If data was written.</returns>
		internal async Task<bool> WriteData(Http2Stream Stream, byte[] Data, int Offset, int Count, bool Last,
			Encoding DataEncoding)
		{
			int StreamId = Stream.StreamId;

			if (Count == 0)
			{
				if (Last)
					return await this.SendHttp2Frame(FrameType.Data, 1, false, Stream, Data, Offset, 0, DataEncoding);   // END_STREAM
				else
					return true;
			}

			int NrBytes;
			byte Flags = 0;

			while (Count > 0)
			{
				if (this.flowControl is null)
					return false;

				NrBytes = await this.flowControl.RequestResources(StreamId, Count);
				if (NrBytes < 0)
					return false;

				if (Last && NrBytes == Count)
					Flags = 1;  // END_STREAM

				if (!await this.SendHttp2Frame(FrameType.Data, Flags, false, Stream, Data, Offset, NrBytes, DataEncoding))
					return false;

				Offset += NrBytes;
				Count -= NrBytes;
			}

			if (Last)
				this.flowControl?.RemoveStream(Stream);

			return true;
		}

		internal Task<bool> SendHttp2Frame(FrameType Type, byte Flags, bool Priority, Http2Stream Stream,
			params byte[] Payload)
		{
			return this.SendHttp2Frame(Type, Flags, Priority, Stream, Payload, 0, Payload.Length, null);
		}

		internal async Task<bool> SendHttp2Frame(FrameType Type, byte Flags, bool Priority,
			Http2Stream Stream, byte[] Payload, int Offset, int Count, Encoding DataEncoding)
		{
			if (Count > 0x00ffffff)
				return false;

			byte[] Data = new byte[9 + Count];
			int StreamId = Stream?.StreamId ?? 0;

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

			if (!await this.client.SendAsync(Data, Priority))
				return false;

			if (this.HasSniffers)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append("TX: ");
				sb.Append(Type.ToString());
				AppendFlags(Type, Flags, sb);
				sb.Append(", Stream ");
				sb.Append(StreamId.ToString());
				sb.Append(", Size ");
				sb.Append(Count.ToString());
				sb.Append(" bytes");

				await this.Information(sb.ToString());

				if (DataEncoding is null)
					await this.TransmitBinary(Data);
				else
				{
					await this.TransmitBinary(BinaryTcpClient.ToArray(Data, 0, 9));
					await this.TransmitText(DataEncoding.GetString(Payload, Offset, Count));
				}
			}

			return true;
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
						this.CloseConnection();
					}
				}
				else
					this.CloseConnection();
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
						this.CloseConnection();
					}
				}
				else
					this.CloseConnection();
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
						this.CloseConnection();
					}
				}
				else
					this.CloseConnection();
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
						this.CloseConnection();
					}
				}
				else
					this.CloseConnection();
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
