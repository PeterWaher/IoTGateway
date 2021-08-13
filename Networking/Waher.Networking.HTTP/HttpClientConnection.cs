using System;
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
using Waher.Runtime.Temporary;
using Waher.Security;

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
		private byte b1 = 0;
		private byte b2 = 0;
		private byte b3 = 0;
		private readonly bool encrypted;
		private bool disposed = false;
		private bool rxText = false;

		internal HttpClientConnection(HttpServer Server, BinaryTcpClient Client, bool Encrypted, params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.server = Server;
			this.client = Client;
			this.encrypted = Encrypted;

			this.client.OnDisconnected += Client_OnDisconnected;
			this.client.OnError += Client_OnError;
			this.client.OnReceived += Client_OnReceived;
		}

		private Task<bool> Client_OnReceived(object Sender, byte[] Buffer, int Offset, int Count)
		{
			this.server.DataReceived(Count);

			if (this.mode == ConnectionMode.Http)
			{
				if (this.header is null)
					return this.BinaryHeaderReceived(Buffer, Offset, Count);
				else
					return this.BinaryDataReceived(Buffer, Offset, Count);
			}
			else
				return this.webSocket?.WebSocketDataReceived(Buffer, Offset, Count) ?? Task.FromResult<bool>(false);
		}

		private Task Client_OnError(object Sender, Exception Exception)
		{
			this.Dispose();
			return Task.CompletedTask;
		}

		private void Client_OnDisconnected(object sender, EventArgs e)
		{
			this.Dispose();
		}

		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				this.webSocket?.Dispose();
				this.webSocket = null;

				this.headerStream?.Dispose();
				this.headerStream = null;

				this.dataStream?.Dispose();
				this.dataStream = null;

				this.client?.DisposeWhenDone();
				this.client = null;

				this.server.Remove(this);
			}
		}

		internal Guid Id
		{
			get { return this.id; }
		}

		internal HttpServer Server
		{
			get { return this.server; }
		}

		internal bool Disposed
		{
			get { return this.disposed; }
		}

		internal BinaryTcpClient Client
		{
			get { return this.client; }
		}

#if !WINDOWS_UWP
		internal bool Encrypted
		{
			get { return this.encrypted; }
		}
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

				if (this.header.HttpVersion < 1)
				{
					await this.SendResponse(null, null, new HttpException(505, "HTTP Version Not Supported", "At least HTTP Version 1.0 is required."), true);
					return false;
				}

				if (this.header.ContentLength != null && (this.header.ContentLength.ContentLength > MaxEntitySize))
				{
					await this.SendResponse(null, null, new HttpException(413, "Request Entity Too Large", "Maximum Entity Size: " + MaxEntitySize.ToString()), true);
					return false;
				}

				if (i + 1 < NrRead)
					return await this.BinaryDataReceived(Data, i + 1, NrRead - i - 1);

				if (!this.header.HasMessageBody)
					return await this.RequestReceived();

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

				await this.SendResponse(null, null, new HttpException(431, "Request Header Fields Too Large", "Max Header Size: " + MaxHeaderSize.ToString()), true);
				return false;
			}
		}

		internal static bool IsSniffableTextType(string ContentType)
		{
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
						case "text/plain":
						case "text/csv":
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
						case "application/json":
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
					switch (ContentType)
					{
						case "multipart/form-data":
							return true;

						default:
							return false;
					}

				default:
					return false;
			}
		}

		private async Task<bool> BinaryDataReceived(byte[] Data, int Offset, int NrRead)
		{
			if (this.dataStream is null)
			{
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
						await this.SendResponse(null, null, new HttpException(NotImplementedException.Code, NotImplementedException.StatusMessage,
							"Transfer encoding not implemented."), false);
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
							await this.SendResponse(null, null, new HttpException(BadRequestException.Code, BadRequestException.StatusMessage, 
								"Negative content lengths invalid."), false);
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
						await this.SendResponse(null, null, new HttpException(411, "Length Required", "Content Length required."), true);
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
						this.ReceiveText(this.rxEncoding.GetString(Data));
					else
						this.ReceiveBinary(Data);
				}
				else
				{
					if (this.rxText)
						this.ReceiveText(this.rxEncoding.GetString(Data, Offset, NrAccepted));
					else
					{
						byte[] Data2 = new byte[NrAccepted];
						Array.Copy(Data, Offset, Data2, 0, NrAccepted);
						this.ReceiveBinary(Data2);
					}
				}
			}

			if (Complete)
			{
				if (this.transferEncoding.InvalidEncoding)
				{
					await this.SendResponse(null, null, new HttpException(BadRequestException.Code, BadRequestException.StatusMessage, 
						"Invalid transfer encoding."), false);
					return true;
				}
				else if (this.transferEncoding.TransferError)
				{
					await this.SendResponse(null, null, new HttpException(InternalServerErrorException.Code, InternalServerErrorException.StatusMessage,
						"Unable to transfer content to resource."), false);
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

				await this.SendResponse(null, null, new HttpException(413, "Request Entity Too Large", "Maximum Entity Size: " + MaxEntitySize.ToString()), true);
				return false;
			}
			else
				return true;
		}

		private async Task<bool> RequestReceived()
		{
#if WINDOWS_UWP
			HttpRequest Request = new HttpRequest(this.header, this.dataStream, 
				this.client.Client.Information.RemoteAddress.ToString() + ":" + this.client.Client.Information.RemotePort);
#else
			HttpRequest Request = new HttpRequest(this.header, this.dataStream, this.client.Client.Client.RemoteEndPoint.ToString());
#endif
			Request.clientConnection = this;

			bool? Queued = await this.QueueRequest(Request);

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

		private async Task<bool?> QueueRequest(HttpRequest Request)
		{
			HttpAuthenticationScheme[] AuthenticationSchemes;
			bool Result;

			try
			{
				if (this.server.TryGetResource(Request.Header.Resource, out HttpResource Resource, out string SubPath))
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
					if (AuthenticationSchemes != null && AuthenticationSchemes.Length > 0)
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

									sb.Append('.');

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
								HttpFieldCookie Cookie = Request.Header.Cookie;
								if (!(Cookie is null))
								{
									string HttpSessionID = Cookie["HttpSessionID"];

									if (!string.IsNullOrEmpty(HttpSessionID))
										Request.Session = this.server.GetSession(HttpSessionID);
								}

								if (Request.Session is null)
								{
									Request.Session = new Script.Variables()
									{
										{ "Global", HttpServer.globalVariables }
									};
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
								Math.Min(this.client.CipherStrength, this.client.HashStrength),
								this.client.KeyExchangeStrength) : 0;
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

							await this.SendResponse(Request, null, new HttpException(401, "Unauthorized", "Unauthorized access prohibited."), false, Challenges.ToArray());
							Request.Dispose();
							return true;
						}
					}

					Resource.Validate(Request);

					if (Request.Header.Expect != null)
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
							await this.SendResponse(Request, null, new HttpException(417, "Expectation Failed", "Unable to parse Expect header."), true);
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
				Result = (Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData);
				await this.SendResponse(Request, null, ex, !Result, ex.HeaderFields);
			}
			catch (System.NotImplementedException ex)
			{
				Result = (Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData);

				Log.Critical(ex);

				await this.SendResponse(Request, null, new NotImplementedException(ex.Message), !Result);
			}
			catch (IOException ex)
			{
				Log.Critical(ex);

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
			catch (Exception ex)
			{
				Result = (Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData);

				Log.Critical(ex);

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
			catch (Exception ex)
			{
				Log.Critical(ex);

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
					Response.Dispose();
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
