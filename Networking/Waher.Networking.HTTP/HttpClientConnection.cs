using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Waher.Content;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.HTTP.TransferEncodings;
using Waher.Security;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Class managing a remote client connection to a local <see cref="HttpServer"/>.
	/// </summary>
	internal class HttpClientConnection : Sniffable, IDisposable
	{
		internal const byte CR = 13;
		internal const byte LF = 10;
		internal const int MaxHeaderSize = 65536;
		internal const int MaxInmemoryMessageSize = 1024 * 1024;	// 1 MB
		internal const long MaxEntitySize = 1024 * 1024 * 1024;		// 1 GB

		private MemoryStream headerStream = null;
		private Stream dataStream = null;
		private TransferEncoding transferEncoding = null;
		private byte[] inputBuffer;
		private HttpServer server;
		private TcpClient client;
		private Stream stream;
		private NetworkStream networkStream;
		private HttpRequestHeader header = null;
		private int bufferSize;
		private byte b1 = 0;
		private byte b2 = 0;
		private byte b3 = 0;

		internal HttpClientConnection(HttpServer Server, TcpClient Client, Stream Stream, NetworkStream NetworkStream, int BufferSize)
		{
			this.server = Server;
			this.client = Client;
			this.stream = Stream;
			this.networkStream = NetworkStream;
			this.bufferSize = BufferSize;
			this.inputBuffer = new byte[this.bufferSize];
			this.BeginRead();
		}

		public void Dispose()
		{
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

			if (this.stream != null)
			{
				this.networkStream.Close(100);
				this.networkStream = null;
				this.stream = null;
			}
		}

		internal void BeginRead()
		{
			this.stream.BeginRead(this.inputBuffer, 0, this.bufferSize, this.BeginReadCallback, null);
		}

		private void BeginReadCallback(IAsyncResult ar)
		{
			try
			{
				int NrRead = this.stream.EndRead(ar);
				bool Continue;

				if (this.header == null)
					Continue = this.BinaryHeaderReceived(this.inputBuffer, 0, NrRead);
				else
					Continue = this.BinaryDataReceived(this.inputBuffer, 0, NrRead);

				if (Continue)
					this.stream.BeginRead(this.inputBuffer, 0, this.bufferSize, this.BeginReadCallback, null);
				else
					this.Dispose();
			}
			catch (SocketException)
			{
				this.Dispose();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
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

				if (this.b1 == CR && this.b2 == LF && this.b3 == CR && b == LF)	// RFC 2616, §2.2
				{
					if (this.headerStream == null)
						Header = InternetContent.ISO_8859_1.GetString(Data, Offset, i - Offset - 3);
					else
					{
						this.headerStream.Write(Data, Offset, i - Offset - 3);
						Header = InternetContent.ISO_8859_1.GetString(this.headerStream.GetBuffer(), 0, (int)this.headerStream.Position);
						this.headerStream = null;
					}
				}
				else if (this.b3 == LF && b == LF)	// RFC 2616, §19.3
				{
					if (this.headerStream == null)
						Header = InternetContent.ISO_8859_1.GetString(Data, Offset, i - Offset - 1);
					else
					{
						this.headerStream.Write(Data, Offset, i - Offset - 1);
						Header = InternetContent.ISO_8859_1.GetString(this.headerStream.GetBuffer(), 0, (int)this.headerStream.Position);
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
				this.header = new HttpRequestHeader(Header);
				if (this.header.HttpVersion < 1)
				{
					this.SendResponse(505, "HTTP Version Not Supported", true);
					return false;
				}
				else if (this.header.ContentLength != null && (this.header.ContentLength.ContentLength > MaxEntitySize))
				{
					this.SendResponse(413, "Request Entity Too Large", true);
					return false;
				}
				else if (i + 1 < NrRead)
					return this.BinaryDataReceived(Data, i + 1, NrRead - i - 1);
				else if (!this.header.HasMessageBody)
					return this.RequestReceived();
				else
					return true;
			}

			if (this.headerStream == null)
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

				this.SendResponse(431, "Request Header Fields Too Large", true);
				return false;
			}
		}

		private bool BinaryDataReceived(byte[] Data, int Offset, int NrRead)
		{
			if (this.dataStream == null)
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
						this.SendResponse(501, "Not Implemented", false);
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
							this.SendResponse(400, "Bad Request", false);
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
						this.SendResponse(411, "Length Required", true);
						return false;
					}
				}
			}

			int NrAccepted;
			bool Complete = this.transferEncoding.Decode(Data, Offset, NrRead, out NrAccepted);
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
					this.SendResponse(400, "Bad Request", false);
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

				this.SendResponse(413, "Request Entity Too Large", true);
				return false;
			}
			else
				return true;
		}

		private bool RequestReceived()
		{
			HttpRequest Request = new HttpRequest(this.header, this.dataStream, this.stream);
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
			HttpResource Resource;
			IUser User;
			string SubPath;
			bool Result;

			try
			{
				if (this.server.TryGetResource(Request.Header.Resource, out Resource, out SubPath))
				{
					AuthenticationSchemes = Resource.GetAuthenticationSchemes(Request);
					if (AuthenticationSchemes != null && AuthenticationSchemes.Length > 0)
					{
						foreach (HttpAuthenticationScheme Scheme in AuthenticationSchemes)
						{
							if (Scheme.IsAuthenticated(Request, out User))
							{
								Request.User = User;
								break;
							}
						}

						if (Request.User == null)
						{
							List<KeyValuePair<string, string>> Challenges = new List<KeyValuePair<string, string>>();

							foreach (HttpAuthenticationScheme Scheme in AuthenticationSchemes)
								Challenges.Add(new KeyValuePair<string, string>("WWW-Authenticate", Scheme.GetChallenge()));

							this.SendResponse(401, "Unauthorized", false, Challenges.ToArray());
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
								this.SendResponse(100, "Continue", false);
								return null;
							}
						}
						else
						{
							this.SendResponse(417, "Expectation Failed", true);
							Request.Dispose();
							return false;
						}
					}

					ThreadPool.QueueUserWorkItem(this.ProcessRequest, new object[] { Request, Resource });
					return true;
				}
				else
				{
					this.SendResponse(404, "Not Found", false);
					Result = true;
				}
			}
			catch (HttpException ex)
			{
				Result = (Request.Header.Expect == null || !Request.Header.Expect.Continue100 || Request.HasData);
				this.SendResponse(ex.StatusCode, ex.Message, !Result, ex.HeaderFields);
			}
			catch (System.NotImplementedException ex)
			{
				Result = (Request.Header.Expect == null || !Request.Header.Expect.Continue100 || Request.HasData);

				Log.Critical(ex);

				this.SendResponse(501, "Not Implemented", !Result);
			}
			catch (IOException ex)
			{
				Log.Critical(ex);

				int Win32ErrorCode = Marshal.GetHRForException(ex) & 0xFFFF;	// TODO: Update to ex.HResult when upgrading to .NET 4.5
				if (Win32ErrorCode == 0x27 || Win32ErrorCode == 0x70)	// ERROR_HANDLE_DISK_FULL, ERROR_DISK_FULL
					this.SendResponse(507, "Insufficient Storage", true);
				else
					this.SendResponse(500, "Internal Server Error", true);

				Result = false;
			}
			catch (Exception ex)
			{
				Result = (Request.Header.Expect == null || !Request.Header.Expect.Continue100 || Request.HasData);

				Log.Critical(ex);

				this.SendResponse(500, "Internal Server Error", !Result);
			}

			Request.Dispose();
			return Result;
		}

		private void ProcessRequest(object State)
		{
			object[] P = (object[])State;
			HttpRequest Request = (HttpRequest)P[0];
			HttpResource Resource = (HttpResource)P[1];
			HttpResponse Response = null;

			try
			{
				Response = new HttpResponse(this.stream, this);
				Resource.Execute(Request, Response);
			}
			catch (HttpException ex)
			{
				if (Response == null || !Response.HeaderSent)
					this.SendResponse(ex.StatusCode, ex.Message, true, ex.HeaderFields);
				else
					this.stream.Close();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				if (Response == null || !Response.HeaderSent)
					this.SendResponse(500, "Internal Server Error", true);
				else
					this.stream.Close();
			}
			finally
			{
				Request.Dispose();
			}
		}

		private void SendResponse(int Code, string Message, bool CloseAfterTransmission, params KeyValuePair<string, string>[] HeaderFields)
		{
			HttpResponse Response = new HttpResponse(this.stream, this);

			Response.StatusCode = Code;
			Response.StatusMessage = Message;
			Response.ContentLength = null;
			Response.ContentType = null;
			Response.ContentLanguage = null;

			foreach (KeyValuePair<string, string> P in HeaderFields)
				Response.SetHeader(P.Key, P.Value);

			if (CloseAfterTransmission)
				Response.SetHeader("Connection", "close");

			Response.SendResponse();

			// TODO: Add error message content.
		}
	}
}
