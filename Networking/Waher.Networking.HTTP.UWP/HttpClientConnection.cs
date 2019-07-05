using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;
using Waher.Content;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.HTTP.TransferEncodings;
using Waher.Networking.HTTP.WebSockets;
using Waher.Security;
#if WINDOWS_UWP
using Windows.Networking.Sockets;
#endif

namespace Waher.Networking.HTTP
{
	internal enum ConnectionMode
	{
		Http,
		WebSocket
	}

	/// <summary>
	/// Class managing a remote client connection to a local <see cref="HttpServer"/>.
	/// </summary>
	internal class HttpClientConnection : Sniffable, IDisposable
	{
		internal const byte CR = 13;
		internal const byte LF = 10;
		internal const int MaxHeaderSize = 65536;
		internal const int MaxInmemoryMessageSize = 1024 * 1024;    // 1 MB
		internal const long MaxEntitySize = 1024 * 1024 * 1024;     // 1 GB

		private MemoryStream headerStream = null;
		private Stream dataStream = null;
		private TransferEncoding transferEncoding = null;
		private readonly byte[] inputBuffer;
		private readonly HttpServer server;
#if WINDOWS_UWP
		private readonly StreamSocket client;
		private Stream inputStream;
		private Stream outputStream;
#else
		private readonly TcpClient client;
		private Stream stream;
		private NetworkStream networkStream;
#endif
		private HttpRequestHeader header = null;
		private ConnectionMode mode = ConnectionMode.Http;
		private WebSocket webSocket = null;
		private string lastResource = string.Empty;
		private readonly int bufferSize;
		private byte b1 = 0;
		private byte b2 = 0;
		private byte b3 = 0;
		private readonly bool encrypted;
		private bool disposed = false;

#if WINDOWS_UWP
		internal HttpClientConnection(HttpServer Server, StreamSocket Client, int BufferSize, bool Encrypted, params ISniffer[] Sniffers)
#else
		internal HttpClientConnection(HttpServer Server, TcpClient Client, Stream Stream, NetworkStream NetworkStream, int BufferSize,
			bool Encrypted, params ISniffer[] Sniffers)
#endif
			: base(Sniffers)
		{
			this.server = Server;
			this.client = Client;
#if WINDOWS_UWP
			this.inputStream = this.client.InputStream.AsStreamForRead(BufferSize);
			this.outputStream = this.client.OutputStream.AsStreamForWrite(BufferSize);
#else
			this.stream = Stream;
			this.networkStream = NetworkStream;
#endif
			this.bufferSize = BufferSize;
			this.encrypted = Encrypted;
			this.inputBuffer = new byte[this.bufferSize];

			Task T = this.BeginRead();
		}

		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				if (this.webSocket != null)
				{
					this.webSocket.Dispose();
					this.webSocket = null;
				}

				if (this.headerStream != null)
				{
					this.headerStream.Dispose();
					this.headerStream = null;
				}

				if (this.dataStream != null)
				{
					this.dataStream.Dispose();
					this.dataStream = null;
				}

#if WINDOWS_UWP
				if (this.outputStream != null)
				{
					this.outputStream.Flush();
					this.outputStream.Dispose();
					this.inputStream = null;
					this.outputStream = null;
				}
#else
				if (this.stream != null)
				{
					this.networkStream.Flush();     // TODO: Wait until sent? Close(100) method not available in .NET Standard 1.3.
					this.networkStream.Dispose();
					this.networkStream = null;
					this.stream = null;
				}
#endif
			}
		}

		/// <summary>
		/// Flushes buffered data out on the network.
		/// </summary>
		public void Flush()
		{
#if WINDOWS_UWP
			if (this.outputStream != null)
				this.outputStream.Flush();
#else
			if (this.networkStream != null)
				this.networkStream.Flush();
#endif
		}

		internal HttpServer Server
		{
			get { return this.server; }
		}

		internal bool Disposed
		{
			get { return this.disposed; }
		}

#if !WINDOWS_UWP
		internal bool Encrypted
		{
			get { return this.encrypted; }
		}
#endif
		internal async Task BeginRead()
		{
			int NrRead;
			bool Continue;

			try
			{
				do
				{
#if WINDOWS_UWP
					NrRead = await this.inputStream.ReadAsync(this.inputBuffer, 0, this.bufferSize);
#else
					NrRead = await this.stream.ReadAsync(this.inputBuffer, 0, this.bufferSize);
#endif
					if (this.disposed)
						return;

					if (NrRead <= 0)
						break;

					this.server.DataReceived(NrRead);

					if (this.mode == ConnectionMode.Http)
					{
						if (this.header is null)
							Continue = this.BinaryHeaderReceived(this.inputBuffer, 0, NrRead);
						else
							Continue = this.BinaryDataReceived(this.inputBuffer, 0, NrRead);
					}
					else
						Continue = this.webSocket?.WebSocketDataReceived(this.inputBuffer, 0, NrRead) ?? false;
				}
				while (Continue);

				this.Dispose();
			}
			catch (Exception)
			{
				this.Dispose();
			}
		}

		private bool BinaryHeaderReceived(byte[] Data, int Offset, int NrRead)
		{
			string Header = null;
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

				this.ReceiveText(Header);
				this.header = new HttpRequestHeader(Header, this.encrypted ? "https" : "http");
				this.lastResource = this.header.Resource;

				if (this.header.HttpVersion < 1)
				{
					this.SendResponse(null, new HttpException(505, "HTTP Version Not Supported", "At least HTTP Version 1.0 is required."), true);
					return false;
				}
				else if (this.header.ContentLength != null && (this.header.ContentLength.ContentLength > MaxEntitySize))
				{
					this.SendResponse(null, new HttpException(413, "Request Entity Too Large", "Maximum Entity Size: " + MaxEntitySize.ToString()), true);
					return false;
				}
				else if (i + 1 < NrRead)
					return this.BinaryDataReceived(Data, i + 1, NrRead - i - 1);
				else if (!this.header.HasMessageBody)
					return this.RequestReceived();
				else
					return true;
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
					this.headerStream.Read(Data2, 0, d);
					this.ReceiveBinary(Data2);
				}

				this.SendResponse(null, new HttpException(431, "Request Header Fields Too Large", "Max Header Size: " + MaxHeaderSize.ToString()), true);
				return false;
			}
		}

		private bool BinaryDataReceived(byte[] Data, int Offset, int NrRead)
		{
			if (this.dataStream is null)
			{
				HttpFieldTransferEncoding TransferEncoding = this.header.TransferEncoding;
				if (TransferEncoding != null)
				{
					if (TransferEncoding.Value == "chunked")
					{
						this.dataStream = new TemporaryFile();
						this.transferEncoding = new ChunkedTransferEncoding(this.dataStream, null);
					}
					else
					{
						this.SendResponse(null, new HttpException(501, "Not Implemented", "Transfer encoding not implemented."), false);
						return true;
					}
				}
				else
				{
					HttpFieldContentLength ContentLength = this.header.ContentLength;
					if (ContentLength != null)
					{
						long l = ContentLength.ContentLength;
						if (l < 0)
						{
							this.SendResponse(null, new HttpException(400, "Bad Request", "Negative content lengths invalid."), false);
							return true;
						}

						if (l <= MaxInmemoryMessageSize)
							this.dataStream = new MemoryStream((int)l);
						else
							this.dataStream = new TemporaryFile();

						this.transferEncoding = new ContentLengthEncoding(this.dataStream, l, null);
					}
					else
					{
						this.SendResponse(null, new HttpException(411, "Length Required", "Content Length required."), true);
						return false;
					}
				}
			}

			bool Complete = this.transferEncoding.Decode(Data, Offset, NrRead, out int NrAccepted);
			if (this.HasSniffers)
			{
				if (Offset == 0 && NrAccepted == Data.Length)
					this.ReceiveBinary(Data);
				else
				{
					byte[] Data2 = new byte[NrAccepted];
					Array.Copy(Data, Offset, Data2, 0, NrAccepted);
					this.ReceiveBinary(Data2);
				}
			}

			if (Complete)
			{
				if (this.transferEncoding.InvalidEncoding)
				{
					this.SendResponse(null, new HttpException(400, "Bad Request", "Invalid transfer encoding."), false);
					return true;
				}
				else
				{
					Offset += NrAccepted;
					NrRead -= NrAccepted;

					if (!this.RequestReceived())
						return false;

					if (NrRead > 0)
						return this.BinaryHeaderReceived(Data, Offset, NrRead);
					else
						return true;
				}
			}
			else if (this.dataStream.Position > MaxEntitySize)
			{
				this.dataStream.Dispose();
				this.dataStream = null;

				this.SendResponse(null, new HttpException(413, "Request Entity Too Large", "Maximum Entity Size: "+MaxEntitySize.ToString()), true);
				return false;
			}
			else
				return true;
		}

		private bool RequestReceived()
		{
#if WINDOWS_UWP
			HttpRequest Request = new HttpRequest(this.header, this.dataStream, this.outputStream,
				this.client.Information.RemoteAddress.ToString() + ":" + this.client.Information.RemotePort);
#else
			HttpRequest Request = new HttpRequest(this.header, this.dataStream, this.stream, this.client.Client.RemoteEndPoint.ToString());
#endif
			Request.clientConnection = this;

			bool? Queued = this.QueueRequest(Request);

			if (Queued.HasValue)
			{
				if (!Queued.Value && this.dataStream != null)
					this.dataStream.Dispose();

				this.header = null;
				this.dataStream = null;
				this.transferEncoding = null;

				return Queued.Value;
			}
			else
				return true;
		}

		private bool? QueueRequest(HttpRequest Request)
		{
			HttpAuthenticationScheme[] AuthenticationSchemes;
			bool Result;

			try
			{
				if (this.server.TryGetResource(Request.Header.Resource, out HttpResource Resource, out string SubPath))
				{
					Request.Resource = Resource;
#if WINDOWS_UWP
					this.server.RequestReceived(Request, this.client.Information.RemoteAddress.ToString() + ":" + this.client.Information.RemotePort, 
						Resource, SubPath);
#else
					this.server.RequestReceived(Request, this.client.Client.RemoteEndPoint.ToString(), Resource, SubPath);
#endif

					AuthenticationSchemes = Resource.GetAuthenticationSchemes(Request);
					if (AuthenticationSchemes != null && AuthenticationSchemes.Length > 0)
					{
						foreach (HttpAuthenticationScheme Scheme in AuthenticationSchemes)
						{
							if (Scheme.IsAuthenticated(Request, out IUser User))
							{
								Request.User = User;
								break;
							}
						}

						if (Request.User is null)
						{
							List<KeyValuePair<string, string>> Challenges = new List<KeyValuePair<string, string>>();

							foreach (HttpAuthenticationScheme Scheme in AuthenticationSchemes)
								Challenges.Add(new KeyValuePair<string, string>("WWW-Authenticate", Scheme.GetChallenge()));

							this.SendResponse(Request, new HttpException(401, "Unauthorized", "Unauthorized access prohibited."), false, Challenges.ToArray());
							Request.Dispose();
							return true;
						}
					}

					Request.SubPath = SubPath;
					Resource.Validate(Request);

					if (Request.Header.Expect != null)
					{
						if (Request.Header.Expect.Continue100)
						{
							if (!Request.HasData)
							{
								this.SendResponse(Request, new HttpException(100, "Continue", null), false);
								return null;
							}
						}
						else
						{
							this.SendResponse(Request, new HttpException(417, "Expectation Failed", "Unable to parse Expect header."), true);
							Request.Dispose();
							return false;
						}
					}

					Task.Run(() => this.ProcessRequest(Request, Resource));
					return true;
				}
				else
				{
					this.SendResponse(Request, new NotFoundException("Resource not found: "+this.server.CheckResourceOverride(Request.Header.Resource)), false);
					Result = true;
				}
			}
			catch (HttpException ex)
			{
				Result = (Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData);
				this.SendResponse(Request, ex, !Result, ex.HeaderFields);
			}
			catch (System.NotImplementedException ex)
			{
				Result = (Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData);

				Log.Critical(ex);

				this.SendResponse(Request, new NotImplementedException(ex.Message), !Result);
			}
			catch (IOException ex)
			{
				Log.Critical(ex);

				int Win32ErrorCode = ex.HResult & 0xFFFF;
				if (Win32ErrorCode == 0x27 || Win32ErrorCode == 0x70)   // ERROR_HANDLE_DISK_FULL, ERROR_DISK_FULL
					this.SendResponse(Request, new HttpException(507, "Insufficient Storage", "Insufficient space."), true);
				else
					this.SendResponse(Request, new InternalServerErrorException(ex.Message), true);

				Result = false;
			}
			catch (Exception ex)
			{
				Result = (Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData);

				Log.Critical(ex);

				this.SendResponse(Request, new InternalServerErrorException(ex.Message), !Result);
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

		private void ProcessRequest(HttpRequest Request, HttpResource Resource)
		{
			HttpResponse Response = null;

			try
			{
#if WINDOWS_UWP
				Response = new HttpResponse(this.outputStream, this, this.server, Request);
#else
				Response = new HttpResponse(this.stream, this, this.server, Request);

				HttpRequestHeader Header = Request.Header;
				int? UpgradePort = null;

				if (!this.encrypted &&
					(Header.UpgradeInsecureRequests?.Upgrade ?? false) &&
					Header.Host != null &&
					string.Compare(Header.Host.Value, "localhost", true) != 0 &&
					((UpgradePort = this.server.UpgradePort).HasValue))
				{
					StringBuilder Location = new StringBuilder();
					string s;
					int i;

					Location.Append("https://");

					s = Header.Host.Value;
					i = s.IndexOf(':');
					if (i > 0)
						s = s.Substring(0, i);

					Location.Append(s);

					if (UpgradePort.Value != HttpServer.DefaultHttpsPort)
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

					this.SendResponse(Request, new HttpException(307, "Moved Temporarily", null), false,
						new KeyValuePair<string, string>("Location", Location.ToString()),
						new KeyValuePair<string, string>("Vary", "Upgrade-Insecure-Requests"));
				}
				else
#endif
					Resource.Execute(this.server, Request, Response);
			}
			catch (HttpException ex)
			{
				if (Response is null || !Response.HeaderSent)
				{
					try
					{
						this.SendResponse(Request, ex, false, this.Merge(ex.HeaderFields, Response.Cookies));
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
				Log.Critical(ex);

				if (Response is null || !Response.HeaderSent)
				{
					try
					{
						this.SendResponse(Request, new InternalServerErrorException(ex.Message), true);
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
#if WINDOWS_UWP
			if (this.outputStream != null)
			{
				try
				{
					this.outputStream.Flush();
				}
				catch (Exception)
				{
					// Ignore.
				}
			}

			if (this.client != null)
			{
				try
				{
					this.client.Dispose();
				}
				catch (Exception)
				{
					// Ignore.
				}
			}
#else
			if (this.stream != null)
			{
				try
				{
					this.stream.Flush();
					this.stream.Dispose();
				}
				catch (Exception)
				{
					// Ignore.
				}
			}
#endif
		}

		private void SendResponse(HttpRequest Request, HttpException ex, bool CloseAfterTransmission, 
			params KeyValuePair<string, string>[] HeaderFields)
		{
#if WINDOWS_UWP
			using (HttpResponse Response = new HttpResponse(this.outputStream, this, this.server, Request)
#else
			using (HttpResponse Response = new HttpResponse(this.stream, this, this.server, Request)
#endif
			{
				StatusCode = ex.StatusCode,
				StatusMessage = ex.Message,
				ContentLength = null,
				ContentType = null,
				ContentLanguage = null
			})
			{
				foreach (KeyValuePair<string, string> P in HeaderFields)
					Response.SetHeader(P.Key, P.Value);

				if (CloseAfterTransmission)
				{
					Response.CloseAfterResponse = true;
					Response.SetHeader("Connection", "close");
				}

                if (ex is null)
                    Response.SendResponse();
                else
                    Response.SendResponse(ex);
            }
        }

		/// <summary>
		/// Underlying stream
		/// </summary>
#if WINDOWS_UWP
		public Stream Stream => this.inputStream;
#else
		public Stream Stream => this.stream;
#endif

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

#if WINDOWS_UWP
				return true;
#else
				if (!this.client.Connected)
					return false;

				// https://msdn.microsoft.com/en-us/library/system.net.sockets.socket.connected.aspx

				bool BlockingBak = this.client.Client.Blocking;
				try
				{
					byte[] Temp = new byte[1];

					this.client.Client.Blocking = false;
					this.client.Client.Send(Temp, 0, 0);

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
					this.client.Client.Blocking = BlockingBak;
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
