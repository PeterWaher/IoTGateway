using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.HTTP.TransferEncodings;
using Waher.Security;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Class managing a remote client connection to a local <see cref="HttpServer"/>.
	/// </summary>
	internal class HttpClientConnection
	{
		private const byte CR = 13;
		private const byte LF = 10;
		private const int MaxHeaderSize = 65536;
		private const int MaxInmemoryMessageSize = 1024 * 1024;

		private MemoryStream headerStream = null;
		private Stream dataStream = null;
		private TransferEncoding transferEncoding = null;
		private byte[] inputBuffer;
		private HttpServer server;
		private TcpClient client;
		private Stream stream;
		private HttpRequestHeader header = null;
		private int bufferSize;
		private byte b1 = 0;
		private byte b2 = 0;
		private byte b3 = 0;

		internal HttpClientConnection(HttpServer Server, TcpClient Client, Stream Stream, int BufferSize)
		{
			this.server = Server;
			this.client = Client;
			this.stream = Stream;
			this.bufferSize = BufferSize;
			this.inputBuffer = new byte[this.bufferSize];
			this.BeginRead();
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
			}
			catch (SocketException)
			{
				// Closed by remote end. Ignore.
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
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
						Header = Encoding.ASCII.GetString(Data, Offset, i - Offset - 3);
					else
					{
						this.headerStream.Write(Data, Offset, i - Offset - 3);
						Header = Encoding.ASCII.GetString(this.headerStream.GetBuffer(), 0, (int)this.headerStream.Position);
						this.headerStream = null;
					}
				}
				else if (this.b3 == LF && b == LF)	// RFC 2616, §19.3
				{
					if (this.headerStream == null)
						Header = Encoding.ASCII.GetString(Data, Offset, i - Offset - 1);
					else
					{
						this.headerStream.Write(Data, Offset, i - Offset - 1);
						Header = Encoding.ASCII.GetString(this.headerStream.GetBuffer(), 0, (int)this.headerStream.Position);
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

				this.header = new HttpRequestHeader(Header);
				if (this.header.HttpVersion < 0)
				{
					this.SendResponse(400, "Bad Request", true);
					return false;
				}
				else if (i + 1 < NrRead)
					return this.BinaryDataReceived(Data, i + 1, NrRead - i - 1);
				else if (!this.header.HasMessageBody)
				{
					this.RequestReceived();
					return true;
				}
				else
					return true;
			}

			return true;
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
						this.transferEncoding = new ChunkedTransferEncoding(this.dataStream);
					}
					else
					{
						this.SendResponse(501, "Not Implemented", true);
						return false;
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
							this.SendResponse(400, "Bad Request", true);
							return false;
						}

						if (l <= MaxInmemoryMessageSize)
							this.dataStream = new MemoryStream((int)l);
						else
							this.dataStream = new TemporaryFile();

						this.transferEncoding = new ContentLengthEncoding(this.dataStream, l);
					}
					else
					{
						this.SendResponse(400, "Bad Request", true);
						return false;
					}
				}
			}

			int NrAccepted;

			if (this.transferEncoding.Decode(Data, Offset, NrRead, out NrAccepted))
			{
				if (this.transferEncoding.InvalidEncoding)
				{
					this.SendResponse(400, "Bad Request", true);
					return false;
				}
				else
				{
					Offset += NrAccepted;
					NrRead -= NrAccepted;

					this.RequestReceived();

					if (NrRead > 0)
						return this.BinaryHeaderReceived(Data, Offset, NrRead);
					else
						return true;
				}
			}
			else
				return true;
		}

		private void RequestReceived()
		{
			HttpRequest Request = new HttpRequest(this.header, this.dataStream, this.stream);

			if (this.QueueRequest(Request))
			{
				this.header = null;
				this.dataStream = null;
				this.transferEncoding = null;
			}
		}

		private bool QueueRequest(HttpRequest Request)
		{
			HttpAuthenticationScheme[] AuthenticationSchemes;
			HttpResource Resource;
			IUser User;
			string SubPath;

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

					Resource.Validate(Request);

					if (Request.Header.Expect != null)
					{
						if (Request.Header.Expect.Continue100)
						{
							if (!Request.HasData)
							{
								this.SendResponse(100, "Continue", false);
								return false;
							}
						}
						else
						{
							this.SendResponse(417, "Expectation Failed", false);
							return true;
						}
					}
					
					Request.SubPath = SubPath;
					ThreadPool.QueueUserWorkItem(this.ProcessRequest, new object[] { Request, Resource });
					return true;
				}
				else
					this.SendResponse(404, "Not Found", true);
			}
			catch (HttpException ex)
			{
				this.SendResponse(ex.StatusCode, ex.Message, true, ex.HeaderFields);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				this.SendResponse(500, "Internal Server Error", true);
			}

			Request.Dispose();
			return true;
		}

		private void ProcessRequest(object State)
		{
			object[] P = (object[])State;
			HttpRequest Request = (HttpRequest)P[0];
			HttpResource Resource = (HttpResource)P[1];
			HttpResponse Response = null;

			try
			{
				Response = new HttpResponse(this.stream);
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
			HttpResponse Response = new HttpResponse(this.stream);

			Response.StatusCode = Code;
			Response.StatusMessage = Message;
			Response.ContentLength = null;
			Response.ContentType = null;
			Response.ContentLanguage = null;

			foreach (KeyValuePair<string, string> P in HeaderFields)
				Response.SetHeader(P.Key, P.Value);

			Response.SendResponse();

			// TODO: Close connection after successful transmission.
			// TODO: Add error message content.
		}

		// TODO: Complete list of HTTP exception classes.
	}
}
