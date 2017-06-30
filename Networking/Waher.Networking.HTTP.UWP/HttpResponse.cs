using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP.TransferEncodings;
using Waher.Networking.Sniffers;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Represets a response of an HTTP client request.
	/// </summary>
	public class HttpResponse : TextWriter
	{
		private const int DefaultChunkSize = 32768;

		private HttpClientConnection clientConnection;
		private Dictionary<string, string> customHeaders = null;
		private LinkedList<Cookie> cookies = null;
		private Encoding encoding = Encoding.UTF8;
		private DateTimeOffset date = DateTimeOffset.Now;
		private DateTimeOffset? expires = null;
		private DateTime lastPing = DateTime.Now;
		private string server = null;
		private string contentLanguage = null;
		private string contentType = null;
		private string statusMessage = "OK";
		private long? contentLength = null;
		private int statusCode = 200;
		private bool responseSent = false;
		private bool onlyHeader = false;

		private Stream responseStream;
		private TransferEncoding transferEncoding = null;
		private TransferEncoding desiredTransferEncoding = null;
		private HttpServer httpServer;
		private HttpRequest httpRequest;

		/// <summary>
		/// Represets a response of an HTTP client request.
		/// </summary>
		public HttpResponse()
			: base()
		{
			this.responseStream = null;
			this.clientConnection = null;
			this.httpServer = null;
			this.httpRequest = null;
		}

		/// <summary>
		/// Represets a response of an HTTP client request.
		/// </summary>
		/// <param name="TransferEncoding">Transfer encoding to use for transfering content to client.</param>
		/// <param name="HttpServer">HTTP Server serving the request.</param>
		/// <param name="Request">Request being served.</param>
		public HttpResponse(TransferEncoding TransferEncoding, HttpServer HttpServer, HttpRequest Request)
			: base()
		{
			this.responseStream = null;
			this.clientConnection = null;
			this.desiredTransferEncoding = TransferEncoding;
			this.httpServer = HttpServer;
			this.httpRequest = Request;
		}

		/// <summary>
		/// Represets a response of an HTTP client request.
		/// </summary>
		/// <param name="ResponseStream">Underlying response stream.</param>
		/// <param name="ClientConnection">Client connection.</param>
		/// <param name="HttpServer">HTTP Server serving the request.</param>
		/// <param name="Request">Request being served.</param>
		internal HttpResponse(Stream ResponseStream, HttpClientConnection ClientConnection, 
			HttpServer HttpServer, HttpRequest Request)
			: base()
		{
			this.responseStream = ResponseStream;
			this.clientConnection = ClientConnection;
			this.httpServer = HttpServer;
			this.httpRequest = Request;
		}

		private void AssertHeaderOpen()
		{
			if (this.transferEncoding != null)
				throw new ArgumentException("Response header written, and cannot be changed.");
		}

		/// <summary>
		/// The Date general-header field represents the date and time at which the message was originated.
		/// </summary>
		public DateTimeOffset Date
		{
			get
			{
				return this.date;
			}

			set
			{
				this.AssertHeaderOpen();
				this.date = value;
			}
		}

		/// <summary>
		/// The Expires entity-header field gives the date/time after which the response is considered stale.
		/// </summary>
		public DateTimeOffset? Expires
		{
			get
			{
				return this.expires;
			}

			set
			{
				this.AssertHeaderOpen();
				this.expires = value;
			}
		}

		/// <summary>
		/// The Server response-header field contains information about the software used by the origin server to handle the request.
		/// </summary>
		public string Server
		{
			get
			{
				return this.server;
			}

			set
			{
				this.AssertHeaderOpen();
				this.server = value;
			}
		}

		/// <summary>
		/// The Content-Language entity-header field describes the natural language(s) of the intended audience for the enclosed entity.
		/// </summary>
		public string ContentLanguage
		{
			get
			{
				return this.contentLanguage;
			}

			set
			{
				this.AssertHeaderOpen();
				this.contentLanguage = value;
			}
		}

		/// <summary>
		/// The Content-Type entity-header field indicates the media type of the entity-body sent to the recipient or, in the case of the HEAD method, 
		/// the media type that would have been sent had the request been a GET. 
		/// </summary>
		public string ContentType
		{
			get
			{
				return this.contentType;
			}

			set
			{
				this.AssertHeaderOpen();
				this.contentType = value;
			}
		}

		/// <summary>
		/// The Content-Length entity-header field indicates the size of the entity-body, in decimal number of OCTETs, sent to the recipient or, 
		/// in the case of the HEAD method, the size of the entity-body that would have been sent had the request been a GET. 
		/// </summary>
		public long? ContentLength
		{
			get
			{
				return this.contentLength;
			}

			set
			{
				this.AssertHeaderOpen();
				this.contentLength = value;
			}
		}

		/// <summary>
		/// HTTP Status code.
		/// </summary>
		public int StatusCode
		{
			get
			{
				return this.statusCode;
			}

			set
			{
				this.AssertHeaderOpen();
				this.statusCode = value;
			}
		}

		/// <summary>
		/// HTTP Status Message.
		/// </summary>
		public string StatusMessage
		{
			get
			{
				return this.statusMessage;
			}

			set
			{
				this.AssertHeaderOpen();
				this.statusMessage = value;
			}
		}

		/// <summary>
		/// If only the header is of interest.
		/// </summary>
		public bool OnlyHeader
		{
			get
			{
				return this.onlyHeader;
			}

			internal set
			{
				this.AssertHeaderOpen();
				this.onlyHeader = value;
			}
		}

		internal LinkedList<Cookie> Cookies
		{
			get { return this.cookies; }
		}

		/// <summary>
		/// Sets a custom header field value.
		/// </summary>
		/// <param name="FieldName">HTTP Header field name.</param>
		/// <param name="Value">Field value.</param>
		public void SetHeader(string FieldName, string Value)
		{
			this.AssertHeaderOpen();

			switch (FieldName.ToLower())
			{
				case "date":
					DateTimeOffset DTO;
					if (CommonTypes.TryParseRfc822(Value, out DTO))
						this.date = DTO;
					else
						throw new ArgumentException("Value does not conform to RFC 822.", "Value");
					break;

				case "expires":
					if (CommonTypes.TryParseRfc822(Value, out DTO))
						this.expires = DTO;
					else
						throw new ArgumentException("Value does not conform to RFC 822.", "Value");
					break;

				case "server":
					this.server = Value;
					break;

				case "content-language":
					this.contentLanguage = Value;
					break;

				case "content-type":
					this.contentType = Value;
					break;

				case "content-length":
					this.contentLength = long.Parse(Value);
					break;

				default:
					if (this.customHeaders == null)
						this.customHeaders = new Dictionary<string, string>();

					this.customHeaders[FieldName] = Value;
					break;
			}
		}

		/// <summary>
		/// Returns a collection of headers available in the response.
		/// </summary>
		/// <returns>HTTP Headers</returns>
		public KeyValuePair<string, string>[] GetHeaders()
		{
			List<KeyValuePair<string, string>> Headers = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>("Date", CommonTypes.EncodeRfc822(this.date))
			};

			if (this.expires.HasValue)
				Headers.Add(new KeyValuePair<string, string>("Expires", CommonTypes.EncodeRfc822(this.expires.Value)));

			if (!string.IsNullOrEmpty(this.server))
				Headers.Add(new KeyValuePair<string, string>("Server", this.server));

			if (!string.IsNullOrEmpty(this.contentLanguage))
				Headers.Add(new KeyValuePair<string, string>("Content-Language", this.contentLanguage));

			if (!string.IsNullOrEmpty(this.contentType))
				Headers.Add(new KeyValuePair<string, string>("Content-Type", this.contentType));

			if (this.contentLength.HasValue)
				Headers.Add(new KeyValuePair<string, string>("Content-Length", this.contentLength.Value.ToString()));

			if (this.customHeaders != null)
			{
				foreach (KeyValuePair<string, string> P in this.customHeaders)
					Headers.Add(P);
			}

			if (this.cookies != null)
			{
				foreach (Cookie Cookie in this.cookies)
					Headers.Add(new KeyValuePair<string, string>("Set-Cookie", Cookie.ToString()));
			}

			return Headers.ToArray();
		}

		/// <summary>
		/// Gets the System.Text.Encoding in which the output is written.
		/// </summary>
		public override Encoding Encoding
		{
			get { return this.encoding; }
		}

		/// <summary>
		/// Releases the unmanaged resources used by the System.IO.StreamWriter and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		/// <exception cref="System.Text.EncoderFallbackException">The current encoding does not support displaying half of a Unicode surrogate pair.</exception>
		protected override void Dispose(bool disposing)
		{
			if (this.clientConnection != null)
			{
				this.clientConnection.Dispose();
				this.clientConnection = null;
				this.responseStream = null;
			}
			else if (this.responseStream != null)
			{
				this.responseStream.Dispose();
				this.responseStream = null;
			}

			if (!this.responseSent)
			{
				this.responseSent = true;

				if (this.httpServer != null)
				{
					if (this.HeaderSent)
						this.httpServer.RequestResponded(this.httpRequest, this.statusCode);
					else
						this.httpServer.RequestResponded(this.httpRequest, 0);
				}
			}
		}

		/// <summary>
		/// Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.
		/// </summary>
		/// <exception cref="System.ObjectDisposedException">The current writer is closed.</exception>
		/// <exception cref="System.IO.IOException">An I/O error has occurred.</exception>
		/// <exception cref="System.Text.EncoderFallbackException">The current encoding does not support displaying half of a Unicode surrogate pair.</exception>
		public override void Flush()
		{
			if (this.transferEncoding != null)
				this.transferEncoding.Flush();
		}

		/// <summary>
		/// Asynchronously clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
		/// </summary>
		/// <returns>A task that represents the asynchronous flush operation.</returns>
		/// <exception cref="System.ObjectDisposedException">The text writer is disposed.</exception>
		/// <exception cref="System.InvalidOperationException">The writer is currently in use by a previous write operation.</exception>
		public override Task FlushAsync()
		{
			if (this.transferEncoding != null)
				this.transferEncoding.Flush();

			return base.FlushAsync();
		}

		/// <summary>
		/// If the header has been sent.
		/// </summary>
		public bool HeaderSent
		{
			get { return this.transferEncoding != null; }
		}

		/// <summary>
		/// Sends the response back to the client. If the resource is synchronous, there's no need to call this method. Only asynchronous
		/// resources need to call this method explicitly.
		/// </summary>
		public void SendResponse()
		{
			if (!this.responseSent)
			{
				this.responseSent = true;

				if (this.httpServer != null)
					this.httpServer.RequestResponded(this.httpRequest, this.statusCode);

				if (this.transferEncoding == null)
					this.StartSendResponse(false);
				else
					this.transferEncoding.ContentSent();
			}
		}

		/// <summary>
		/// Sends an error response back to the client.
		/// </summary>
		/// <param name="ex">Exception</param>
		public void SendResponse(Exception ex)
		{
			if (this.HeaderSent)
				Log.Critical(ex);
			else
			{
				try
				{
					this.ContentLength = null;
					this.ContentType = null;
					this.ContentLanguage = null;

					if (ex is HttpException ex2)
					{
						this.StatusCode = ex2.StatusCode;
						this.StatusMessage = ex2.Message;

						foreach (KeyValuePair<string, string> P in ex2.HeaderFields)
							this.SetHeader(P.Key, P.Value);

						this.SetHeader("Connection", "close");
						this.SendResponse();
					}
					else if (ex is System.NotImplementedException)
					{
						Log.Critical(ex);

						this.StatusCode = 501;
						this.StatusMessage = "Not Implemented";
						this.SetHeader("Connection", "close");
						this.SendResponse();
					}
					else if (ex is IOException)
					{
						Log.Critical(ex);

						int Win32ErrorCode = Marshal.GetHRForException(ex) & 0xFFFF;    // TODO: Update to ex.HResult when upgrading to .NET 4.5
						if (Win32ErrorCode == 0x27 || Win32ErrorCode == 0x70)   // ERROR_HANDLE_DISK_FULL, ERROR_DISK_FULL
						{
							this.StatusCode = 507;
							this.StatusMessage = "Insufficient Storage";
						}
						else
						{
							this.StatusCode = 500;
							this.StatusMessage = "Internal Server Error";
						}

						this.SetHeader("Connection", "close");
						this.SendResponse();
					}
					else
					{
						Log.Critical(ex);

						this.StatusCode = 500;
						this.StatusMessage = "Internal Server Error";
						this.SetHeader("Connection", "close");
						this.SendResponse();
					}
				}
				catch (Exception)
				{
					Log.Critical(ex);
				}
			}
		}

		private void StartSendResponse(bool ExpectContent)
		{
			if (this.transferEncoding == null)
			{
				if (this.desiredTransferEncoding == null)
				{
					StringBuilder Output = new StringBuilder();

					Output.Append("HTTP/1.1 ");
					Output.Append(this.statusCode.ToString());
					Output.Append(' ');
					Output.Append(this.statusMessage);

					Output.Append("\r\nDate: ");
					Output.Append(CommonTypes.EncodeRfc822(this.date));

					if (this.expires.HasValue)
					{
						Output.Append("\r\nExpires: ");
						Output.Append(CommonTypes.EncodeRfc822(this.expires.Value));
					}

					Output.Append("\r\nServer: ");
					if (string.IsNullOrEmpty(this.server))
						Output.Append("Waher.Networking.HTTP");
					else
						Output.Append(this.server + " (Waher.Networking.HTTP)");

					if (!string.IsNullOrEmpty(this.contentLanguage))
					{
						Output.Append("\r\nContent-Language: ");
						Output.Append(this.contentLanguage);
					}

					if (!string.IsNullOrEmpty(this.contentType))
					{
						Output.Append("\r\nContent-Type: ");
						Output.Append(this.contentType);

						if (this.contentType.StartsWith("text/") && !this.contentType.Contains("charset="))
						{
							Output.Append("; charset=");
							Output.Append(this.encoding.WebName);
						}
					}

					if ((ExpectContent || this.contentLength.HasValue) &&
						((this.statusCode >= 100 && this.statusCode <= 199) || this.statusCode == 204 || this.statusCode == 304))
					{
						throw new Exception("Content not allowed for status codes " + this.statusCode.ToString());

						// When message bodies are required:
						// http://stackoverflow.com/questions/299628/is-an-entity-body-allowed-for-an-http-delete-request
					}

					if (this.contentLength.HasValue)
					{
						Output.Append("\r\nContent-Length: ");
						Output.Append(this.contentLength.Value.ToString());

						this.transferEncoding = new ContentLengthEncoding(this.onlyHeader ? Stream.Null : this.responseStream, this.contentLength.Value, this.clientConnection);
					}
					else if (ExpectContent)
					{
						Output.Append("\r\nTransfer-Encoding: chunked");
						this.transferEncoding = new ChunkedTransferEncoding(this.onlyHeader ? Stream.Null : this.responseStream, DefaultChunkSize, this.clientConnection);
					}
					else
					{
						if ((this.statusCode < 100 || this.statusCode > 199) && this.statusCode != 204 && this.statusCode != 304)
							Output.Append("\r\nContent-Length: 0");

						this.transferEncoding = new ContentLengthEncoding(this.onlyHeader ? Stream.Null : this.responseStream, 0, this.clientConnection);
					}

					if (this.customHeaders != null)
					{
						foreach (KeyValuePair<string, string> P in this.customHeaders)
						{
							Output.Append("\r\n");
							Output.Append(P.Key);
							Output.Append(": ");
							Output.Append(P.Value);
						}
					}

					if (this.cookies != null)
					{
						foreach (Cookie Cookie in this.cookies)
						{
							Output.Append("\r\nSet-Cookie: ");
							Output.Append(Cookie.ToString());
						}
					}

					Output.Append("\r\n\r\n");

					string Header = Output.ToString();
					byte[] HeaderBin = InternetContent.ISO_8859_1.GetBytes(Header);

					if (this.responseStream == null || this.clientConnection.Disposed)
						return;

					this.responseStream.Write(HeaderBin, 0, HeaderBin.Length);
					this.clientConnection.Server.DataTransmitted(HeaderBin.Length);
					this.clientConnection.TransmitText(Header);
				}
				else
				{
					this.transferEncoding = this.desiredTransferEncoding;
					this.transferEncoding.BeforeContent(this, ExpectContent);
				}
			}
		}

		/// <summary>
		/// Returns an object to the client. This method can only be called once per response, and only as the only method that returns a response
		/// to the client.
		/// </summary>
		/// <param name="Object">Object to return. Object will be encoded using Internet Content encoders, as defined in <see cref="Waher.Content"/>.</param>
		public void Return(object Object)
		{
			byte[] Data = InternetContent.Encode(Object, this.encoding, out string ContentType);

			this.ContentType = ContentType;
			this.ContentLength = Data.Length;

			this.Write(Data);
		}

		/// <summary>
		/// Returns binary data in the response.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		public void Write(byte[] Data)
		{
			DateTime TP;

			if (this.transferEncoding == null)
				this.StartSendResponse(true);

			this.transferEncoding.Encode(Data, 0, Data.Length);

			if (this.httpServer != null && ((TP = DateTime.Now) - this.lastPing).TotalSeconds >= 1)
			{
				this.lastPing = TP;
				this.httpServer.PingRequest(this.httpRequest);
			}
		}

		/// <summary>
		/// Returns binary data in the response.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <param name="Offset">Offset into <paramref name="Data"/>.</param>
		/// <param name="Count">Number of bytes to return.</param>
		public void Write(byte[] Data, int Offset, int Count)
		{
			DateTime TP;

			if (this.transferEncoding == null)
				this.StartSendResponse(true);

			this.transferEncoding.Encode(Data, Offset, Count);

			if (this.httpServer != null && ((TP = DateTime.Now) - this.lastPing).TotalSeconds >= 1)
			{
				this.lastPing = TP;
				this.httpServer.PingRequest(this.httpRequest);
			}
		}

		/// <summary>
		/// Writes a character to the stream.
		/// </summary>
		/// <param name="value">The character to write to the text stream.</param>
		/// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
		/// <exception cref="System.ObjectDisposedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer is full, 
		/// and current writer is closed.</exception>
		/// <exception cref="System.NotSupportedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the System.IO.StreamWriter 
		/// is at the end the stream.</exception>
		public override void Write(char value)
		{
			this.Write(this.encoding.GetBytes(new char[] { value }));
		}

		/// <summary>
		/// Writes a character array to the stream.
		/// </summary>
		/// <param name="buffer">A character array containing the data to write. If buffer is null, nothing is written.</param>
		/// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
		/// <exception cref="System.ObjectDisposedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and current writer is closed.</exception>
		/// <exception cref="System.NotSupportedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the System.IO.StreamWriter 
		/// is at the end the stream.</exception>
		public override void Write(char[] buffer)
		{
			this.Write(this.encoding.GetBytes(buffer));
		}

		/// <summary>
		/// Writes a string to the stream.
		/// </summary>
		/// <param name="value">The string to write to the stream. If value is null, nothing is written.</param>
		/// <exception cref="System.ObjectDisposedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and current writer is closed.</exception>
		/// <exception cref="System.NotSupportedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the System.IO.StreamWriter 
		/// is at the end the stream.</exception>
		/// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
		public override void Write(string value)
		{
			this.Write(this.encoding.GetBytes(value));
		}

		/// <summary>
		/// Writes a subarray of characters to the stream.
		/// </summary>
		/// <param name="buffer">A character array containing the data to write.</param>
		/// <param name="index">The index into buffer at which to begin writing.</param>
		/// <param name="count">The number of characters to read from buffer.</param>
		/// <exception cref="System.ArgumentNullException">buffer is null.</exception>
		/// <exception cref="System.ArgumentException">The buffer length minus index is less than count.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">index or count is negative.</exception>
		/// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
		/// <exception cref="System.ObjectDisposedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and current writer is closed.</exception>
		/// <exception cref="System.NotSupportedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the System.IO.StreamWriter 
		/// is at the end the stream.</exception>
		public override void Write(char[] buffer, int index, int count)
		{
			this.Write(this.encoding.GetBytes(buffer, index, count));
		}

		/// <summary>
		/// Sets a cookie in the response.
		/// </summary>
		/// <param name="Cookie">Cookie.</param>
		public void SetCookie(Cookie Cookie)
		{
			if (this.cookies == null)
				this.cookies = new LinkedList<Cookie>();

			this.cookies.AddLast(Cookie);
		}

		/// <summary>
		/// Sets the response stream of the response. Can only be set, if not set before.
		/// </summary>
		/// <param name="ResponseStream">New response stream.</param>
		/// <exception cref="Exception">If a response stream has already been set.</exception>
		public void SetResponseStream(Stream ResponseStream)
		{
			if (this.responseStream != null)
				throw new Exception("Response stream already set.");

			this.responseStream = ResponseStream;
		}

	}
}
