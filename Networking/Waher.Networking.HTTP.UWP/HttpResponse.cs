using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Binary;
using Waher.Content.Text;
using Waher.Events;
using Waher.Networking.HTTP.ContentEncodings;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.HTTP.HTTP2;
using Waher.Networking.HTTP.TransferEncodings;
using Waher.Runtime.Inventory;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Represets a response of an HTTP client request.
	/// </summary>
	public class HttpResponse : IDisposable
	{
		private const int DefaultChunkSize = 32768;

		private HttpClientConnection clientConnection;
		private Dictionary<string, string> customHeaders = null;
		private LinkedList<Cookie> cookies = null;
		private List<string> challenges = null;
		private readonly Encoding encoding = Encoding.UTF8;
		private readonly Http2Stream http2Stream;
		private bool encodingUsed = false;
		private DateTimeOffset date = DateTimeOffset.Now;
		private DateTimeOffset? expires = null;
		private DateTime lastPing = DateTime.Now;
		private string server = null;
		private string contentLanguage = null;
		private string contentType = null;
		private string eTag = null;
		private string statusMessage = "OK";
		private long? contentLength = null;
		private int statusCode = 200;
		private bool responseSent = false;
		private bool disposed = false;
		private bool onlyHeader = false;
		private bool closeAfterResponse = false;
		private bool txText = false;

		private TransferEncoding transferEncoding = null;
		private IBinaryTransmission responseStream;
		private readonly TransferEncoding desiredTransferEncoding = null;
		private readonly HttpServer httpServer;
		private readonly HttpRequest httpRequest;

		/// <summary>
		/// Represets a response of an HTTP client request.
		/// </summary>
		public HttpResponse()
		{
			this.responseStream = null;
			this.clientConnection = null;
			this.httpServer = null;
			this.httpRequest = null;
			this.http2Stream = null;
		}

		/// <summary>
		/// Represets a response of an HTTP client request.
		/// </summary>
		/// <param name="TransferEncoding">Transfer encoding to use for transfering content to client.</param>
		/// <param name="HttpServer">HTTP Server serving the request.</param>
		/// <param name="Request">Request being served.</param>
		public HttpResponse(TransferEncoding TransferEncoding, HttpServer HttpServer, HttpRequest Request)
		{
			this.responseStream = null;
			this.clientConnection = null;
			this.desiredTransferEncoding = TransferEncoding;
			this.httpServer = HttpServer;
			this.httpRequest = Request;
			this.http2Stream = Request?.Http2Stream;

			if (!(Request is null))
			{
				Request.Response = this;

				if (Request.Header.TryGetHeaderField("Connection", out HttpField Field) && Field.Value == "close")
				{
					this.closeAfterResponse = true;
					this.SetHeader("Connection", "close");
				}
			}
		}

		/// <summary>
		/// Represets a response of an HTTP client request.
		/// </summary>
		/// <param name="ResponseStream">Underlying response stream.</param>
		/// <param name="ClientConnection">Client connection.</param>
		/// <param name="HttpServer">HTTP Server serving the request.</param>
		/// <param name="Request">Request being served.</param>
		internal HttpResponse(IBinaryTransmission ResponseStream, HttpClientConnection ClientConnection, HttpServer HttpServer, HttpRequest Request)
		{
			this.responseStream = ResponseStream;
			this.clientConnection = ClientConnection;
			this.httpServer = HttpServer;
			this.httpRequest = Request;
			this.http2Stream = Request?.Http2Stream;

			if (!(Request is null))
				Request.Response = this;
		}

		private void AssertHeaderOpen()
		{
			if (!(this.transferEncoding is null))
				throw new Exception("Response header written, and cannot be changed.");
		}

		/// <summary>
		/// The Date general-header field represents the date and time at which the message was originated.
		/// </summary>
		public DateTimeOffset Date
		{
			get => this.date;
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
			get => this.expires;
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
			get => this.server;
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
			get => this.contentLanguage;
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
			get => this.contentType;
			set
			{
				this.AssertHeaderOpen();

				if (!string.IsNullOrEmpty(value) && this.statusCode >= 200 && this.statusCode < 300)
					this.Request.Header.AssertAcceptable(value);

				this.contentType = value;
			}
		}

		/// <summary>
		/// The Content-Length entity-header field indicates the size of the entity-body, in decimal number of OCTETs, sent to the recipient or, 
		/// in the case of the HEAD method, the size of the entity-body that would have been sent had the request been a GET. 
		/// </summary>
		public long? ContentLength
		{
			get => this.contentLength;
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
			get => this.statusCode;
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
			get => this.statusMessage;
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
			get => this.onlyHeader;
			internal set
			{
				this.AssertHeaderOpen();
				this.onlyHeader = value;
			}
		}

		/// <summary>
		/// Corresponding HTTP Request
		/// </summary>
		public HttpRequest Request => this.httpRequest;

		internal LinkedList<Cookie> Cookies => this.cookies;

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
					else if (int.TryParse(Value, out int i))
						this.date = DateTimeOffset.UtcNow.AddSeconds(i);
					else
					{
						Log.Error("Value does not conform to RFC 822: " + Value, this.httpRequest.Resource.ResourceName,
							new KeyValuePair<string, object>("Header", FieldName),
							new KeyValuePair<string, object>("Value", Value));

						this.customHeaders ??= new Dictionary<string, string>();
						this.customHeaders[FieldName] = Value;
					}
					break;

				case "expires":
					if (CommonTypes.TryParseRfc822(Value, out DTO))
						this.expires = DTO;
					else if (int.TryParse(Value, out int i))
						this.expires = DateTimeOffset.UtcNow.AddSeconds(i);
					else
					{
						Log.Error("Value does not conform to RFC 822: " + Value, this.httpRequest.Resource.ResourceName,
							new KeyValuePair<string, object>("Header", FieldName),
							new KeyValuePair<string, object>("Value", Value));

						this.customHeaders ??= new Dictionary<string, string>();
						this.customHeaders[FieldName] = Value;
					}
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

				case "www-authenticate":
					this.challenges ??= new List<string>();
					this.challenges.Add(Value);
					break;

				case "etag":
					if (Value.Length > 1 && Value[0] == '"' && Value[^1] == '"')
						this.eTag = Value[1..^1];
					else
						this.eTag = Value;
					break;

				default:
					this.customHeaders ??= new Dictionary<string, string>();
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

			if (!(this.challenges is null))
			{
				foreach (string Challenge in this.challenges)
					Headers.Add(new KeyValuePair<string, string>("WWW-Authenticate", Challenge));
			}

			if (!string.IsNullOrEmpty(this.eTag))
				Headers.Add(new KeyValuePair<string, string>("ETag", this.eTag));

			if (!(this.customHeaders is null))
			{
				foreach (KeyValuePair<string, string> P in this.customHeaders)
					Headers.Add(P);
			}

			if (!(this.cookies is null))
			{
				foreach (Cookie Cookie in this.cookies)
					Headers.Add(new KeyValuePair<string, string>("Set-Cookie", Cookie.ToString()));
			}

			return Headers.ToArray();
		}

		/// <summary>
		/// Gets the first header value matching a given header field name.
		/// </summary>
		/// <param name="FieldName">Header field name.</param>
		/// <returns>First header value correspondig to <paramref name="FieldName"/>.</returns>
		public string GetFirstHeader(string FieldName)
		{
			switch (FieldName.ToUpper())
			{
				case "DATE": return CommonTypes.EncodeRfc822(this.date);
				case "EXPIRES": return this.expires.HasValue ? CommonTypes.EncodeRfc822(this.expires.Value) : null;
				case "SERVER": return this.server;
				case "CONTENT-LANGUAGE": return this.contentLanguage;
				case "CONTENT-TYPE": return this.contentType;
				case "CONTENT-LENGTH": return this.contentLength.HasValue ? this.contentLength.Value.ToString() : null;
				case "ETAG": return this.eTag;

				case "SET-COOKIE":
					if (!(this.cookies is null))
					{
						foreach (Cookie Cookie in this.cookies)
							return Cookie.ToString();
					}

					return null;

				case "WWW-AUTHENTICATE":
					return this.challenges?[0];

				default:
					if (!(this.customHeaders is null))
					{
						foreach (KeyValuePair<string, string> P in this.customHeaders)
						{
							if (string.Compare(P.Key, FieldName, true) == 0)
								return P.Value;
						}
					}

					return null;
			}
		}

		/// <summary>
		/// Gets the System.Text.Encoding in which the output is written.
		/// </summary>
		public Encoding Encoding => this.encoding;

		/// <summary>
		/// If the connection should be closed after the response has been sent.
		/// </summary>
		public bool CloseAfterResponse
		{
			get => this.closeAfterResponse;
			set => this.closeAfterResponse = value;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the System.IO.StreamWriter and optionally releases the managed resources.
		/// </summary>
		/// <exception cref="EncoderFallbackException">The current encoding does not support displaying half of a Unicode surrogate pair.</exception>
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
			this.disposed = true;

			if (this.closeAfterResponse && !(this.clientConnection is null))
			{
				await this.clientConnection.DisposeAsync();
				this.clientConnection = null;
			}

			if (!this.responseSent)
			{
				this.responseSent = true;

				if (!(this.httpServer is null) && !(this.httpRequest is null))
				{
					if (this.HeaderSent)
						this.httpServer.RequestResponded(this.httpRequest, this.statusCode);
					else
						this.httpServer.RequestResponded(this.httpRequest, 0);
				}
			}

			this.transferEncoding?.Dispose();
			this.transferEncoding = null;
		}

		/// <summary>
		/// Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The current writer is closed.</exception>
		/// <exception cref="IOException">An I/O error has occurred.</exception>
		/// <exception cref="EncoderFallbackException">The current encoding does not support displaying half of a Unicode surrogate pair.</exception>
		public Task Flush()
		{
			return this.transferEncoding?.FlushAsync() ?? Task.CompletedTask;
		}

		/// <summary>
		/// Asynchronously clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
		/// </summary>
		/// <returns>A task that represents the asynchronous flush operation.</returns>
		/// <exception cref="ObjectDisposedException">The text writer is disposed.</exception>
		/// <exception cref="InvalidOperationException">The writer is currently in use by a previous write operation.</exception>
		[Obsolete("Use Flush() method instead.")]
		public Task FlushAsync()
		{
			return this.Flush();
		}

		/// <summary>
		/// If the header has been sent.
		/// </summary>
		public bool HeaderSent => !(this.transferEncoding is null);

		/// <summary>
		/// If the response has been sent.
		/// </summary>
		public bool ResponseSent => this.responseSent;

		/// <summary>
		/// If the response has been disposed.
		/// </summary>
		public bool Disposed => this.disposed;

		/// <summary>
		/// Current client connection
		/// </summary>
		public BinaryTcpClient ClientConnection => this.clientConnection?.Client;

		/// <summary>
		/// If the response contains any WWW-Authenticate challenges.
		/// </summary>
		public bool HasChallenges => !(this.challenges is null);

		/// <summary>
		/// Gets available WWW-Authenticate challenges returned in the response.
		/// </summary>
		/// <returns>Challenges</returns>
		public string[] GetChallenges()
		{
			return this.challenges?.ToArray() ?? new string[0];
		}

		/// <summary>
		/// Transfer encoding in response.
		/// </summary>
		public TransferEncoding TransferEncoding => this.transferEncoding ?? this.desiredTransferEncoding;

		/// <summary>
		/// Sends the response back to the client. If the resource is synchronous, there's no need to call this method. Only asynchronous
		/// resources need to call this method explicitly.
		/// </summary>
		public async Task SendResponse()
		{
			if (!this.responseSent)
			{
				this.responseSent = true;

				this.httpServer?.RequestResponded(this.httpRequest, this.statusCode);

				if (this.transferEncoding is null)
					await this.StartSendResponse(false);
				else
					await this.transferEncoding.ContentSentAsync();

				await this.OnResponseSent.Raise(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Sends an error response back to the client.
		/// </summary>
		/// <param name="ex">Exception</param>
		public async Task SendResponse(Exception ex)
		{
			ex = Log.UnnestException(ex);

			if (this.HeaderSent)
				this.clientConnection?.Error(ex.Message);
			else
			{
				try
				{
					this.ContentLength = null;
					this.ContentType = null;
					this.ContentLanguage = null;

					byte[] Content = null;
					string ContentType = null;

					if (ex is HttpException ex2)
					{
						this.StatusCode = ex2.StatusCode;
						this.StatusMessage = ex2.Message;

						if (!(ex2.Content is null))
						{
							ContentType = ex2.ContentType;
							Content = ex2.Content;
						}
						else
						{
							object ContentObject = await ex2.GetContentObjectAsync();

							if (ContentObject is string s)
							{
								Content = Encoding.UTF8.GetBytes(s);
								ContentType = PlainTextCodec.DefaultContentType + "; charset=utf-8";
							}
							else if (!(ContentObject is null))
							{
								EncodingResult Result = await this.TryEncode(ContentObject);
								if (!(Result is null))
								{
									Content = Result.Data;
									ContentType = Result.ContentType;
								}
							}
						}

						if (!(ex2.HeaderFields is null))
						{
							foreach (KeyValuePair<string, string> P in ex2.HeaderFields)
							{
								if (string.Compare(P.Key, "Content-Type", true) == 0)
								{
									if (string.IsNullOrEmpty(ContentType))
										ContentType = P.Value;

									continue;
								}

								this.SetHeader(P.Key, P.Value);
							}
						}
					}
					else
					{
						this.clientConnection?.Error(ex.Message);

						if (ex is System.NotImplementedException)
						{
							this.StatusCode = 501;
							this.StatusMessage = "Not Implemented";
						}
						else if (ex is IOException)
						{
							int Win32ErrorCode = ex.HResult & 0xFFFF;
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
						}
						else
						{
							this.StatusCode = 500;
							this.StatusMessage = "Internal Server Error";
						}

						if (Content is null && !string.IsNullOrEmpty(ex.Message) &&
							((this.statusCode < 100 || this.statusCode > 199) && this.statusCode != 204 && this.statusCode != 304))
						{
							Content = Encoding.UTF8.GetBytes(ex.Message);
							ContentType = PlainTextCodec.DefaultContentType + "; charset=utf-8";
						}
					}

					if (this.statusCode >= 400 && !(this.httpServer is null) && this.httpServer.HasCustomErrors)
					{
						CustomErrorEventArgs e = new CustomErrorEventArgs(this.statusCode, this.statusMessage, ContentType, Content,
							this.httpRequest, this);

						await this.httpServer.CustomizeError(e);
						Content = e.Content;
						ContentType = e.ContentType;
					}

					if (!(Content is null))
					{
						this.ContentType = string.IsNullOrEmpty(ContentType) ? BinaryCodec.DefaultContentType : ContentType;
						this.ContentLength = Content.Length;

						await this.Write(Content);
					}

					await this.SendResponse();
				}
				catch (Exception ex2)
				{
					this.clientConnection?.Error(ex.Message);
					this.clientConnection?.Error(ex2.Message);
				}
			}
		}

		private async Task StartSendResponse(bool ExpectContent)
		{
			if (this.transferEncoding is null)
			{
				if (this.http2Stream is null)
				{
					if (this.desiredTransferEncoding is null)
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

						if (!string.IsNullOrEmpty(this.server))
						{
							Output.Append("\r\nServer: ");
							Output.Append(this.server);
						}
						else if (!string.IsNullOrEmpty(this.httpServer?.Name))
						{
							Output.Append("\r\nServer: ");
							Output.Append(this.httpServer.Name);
						}

						Output.Append("\r\nAlt-Svc: h2");
						if (!this.clientConnection.Encrypted)
							Output.Append('c');
						Output.Append("=\":");
						Output.Append(this.clientConnection.Port.ToString());
						Output.Append("\"; ma=3600");

						if (!string.IsNullOrEmpty(this.contentLanguage))
						{
							Output.Append("\r\nContent-Language: ");
							Output.Append(this.contentLanguage);
						}

						if (!string.IsNullOrEmpty(this.contentType))
						{
							Output.Append("\r\nContent-Type: ");
							Output.Append(this.contentType);

							if (!this.encodingUsed && this.contentType.StartsWith("text/"))
								this.encodingUsed = true;

							if (this.encodingUsed && !this.contentType.Contains("charset="))
							{
								Output.Append("; charset=");
								Output.Append(this.encoding.WebName);
								this.txText = true;
							}
							else if (this.clientConnection?.HasSniffers ?? false)
								this.txText = HttpClientConnection.IsSniffableTextType(this.contentType);
							else
								this.txText = false;

							Output.Append("\r\nX-Content-Type-Options: nosniff");
						}

						if ((ExpectContent || this.contentLength.HasValue) &&
							((this.statusCode >= 100 && this.statusCode <= 199) || this.statusCode == 204 || this.statusCode == 304))
						{
							throw new Exception("Content not allowed for status codes " + this.statusCode.ToString());

							// When message bodies are required:
							// http://stackoverflow.com/questions/299628/is-an-entity-body-allowed-for-an-http-delete-request
						}

						IContentEncoding ContentEncoding = this.httpRequest?.Header?.AcceptEncoding?.TryGetBestContentEncoder(this.contentLength.HasValue ? this.eTag : null);
						bool TxText = ContentEncoding is null && this.txText;

						if (this.contentLength.HasValue && ContentEncoding is null)
						{
							Output.Append("\r\nContent-Length: ");
							Output.Append(this.contentLength.Value.ToString());

							this.transferEncoding = new ContentLengthEncoding(this.onlyHeader ? null : this.responseStream,
								this.contentLength.Value, this.clientConnection, TxText, this.encoding);
						}
						else if (ExpectContent)
						{
							if (!(ContentEncoding is null) &&
								(!this.contentLength.HasValue || (this.contentLength.Value >= 128 && this.contentLength.Value < 128 * 1024 * 1024)) &&
								(string.IsNullOrEmpty(this.contentType) ||
								!(this.contentType.StartsWith("image/") ||
								this.contentType.StartsWith("audio/") ||
								this.contentType.StartsWith("video/") ||
								this.contentType == BinaryCodec.DefaultContentType)))
							{
								Output.Append("\r\nContent-Encoding: ");
								Output.Append(ContentEncoding.Label);

								FileInfo PrecompressedFile = ContentEncoding.TryGetPrecompressedFile(this.contentLength.HasValue ? this.eTag : null);

								if (PrecompressedFile?.Exists ?? false)
								{
									TxText = false;

									Output.Append("\r\nContent-Length: ");
									Output.Append(PrecompressedFile.Length.ToString());

									this.transferEncoding = new ContentLengthEncoding(this.onlyHeader ? null : this.responseStream,
										PrecompressedFile.Length, this.clientConnection, TxText, this.encoding);

									this.transferEncoding = new PrecompressedFileReturner(PrecompressedFile, this.transferEncoding);
								}
								else
								{
									Output.Append("\r\nTransfer-Encoding: chunked");

									this.transferEncoding = new ChunkedTransferEncoding(this.onlyHeader ? null : this.responseStream,
										DefaultChunkSize, this.clientConnection, TxText, this.encoding);

									this.transferEncoding = ContentEncoding.GetEncoder(this.transferEncoding, this.contentLength,
										this.contentLength.HasValue ? this.eTag : null);
								}
							}
							else
							{
								Output.Append("\r\nTransfer-Encoding: chunked");

								this.transferEncoding = new ChunkedTransferEncoding(this.onlyHeader ? null : this.responseStream,
									DefaultChunkSize, this.clientConnection, TxText, this.encoding);
							}
						}
						else
						{
							if ((this.statusCode < 100 || this.statusCode > 199) && this.statusCode != 204 && this.statusCode != 304)
								Output.Append("\r\nContent-Length: 0");

							this.transferEncoding = new ContentLengthEncoding(this.onlyHeader ? null : this.responseStream, 0,
								this.clientConnection, TxText, this.encoding);
						}

						if (!(this.challenges is null))
						{
							foreach (string Challenge in this.challenges)
							{
								Output.Append("\r\nWWW-Authenticate: ");
								Output.Append(Challenge);
							}
						}

						if (!string.IsNullOrEmpty(this.eTag))
						{
							Output.Append("\r\nETag: ");
							Output.Append(this.eTag);
						}

						if (!(this.customHeaders is null))
						{
							foreach (KeyValuePair<string, string> P in this.customHeaders)
							{
								Output.Append("\r\n");
								Output.Append(P.Key);
								Output.Append(": ");
								Output.Append(P.Value);
							}
						}

						if (!(this.cookies is null))
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

						if (this.responseStream is null || this.clientConnection.Disposed)
							return;

						this.responseStream?.SendAsync(HeaderBin, 0, HeaderBin.Length);
						this.clientConnection.Server.DataTransmitted(HeaderBin.Length);

						if (this.clientConnection.HasSniffers)
							await this.clientConnection.TransmitText(Header);
					}
					else
					{
						this.transferEncoding = this.desiredTransferEncoding;
						await this.transferEncoding.BeforeContentAsync(this, ExpectContent);
					}
				}
				else
				{
					Http2TransferEncoding Http2TransferEncoding = new Http2TransferEncoding(this.http2Stream, this.contentLength);
					this.transferEncoding = Http2TransferEncoding;

					HeaderWriter w = this.http2Stream.Connection.HttpHeaderWriter;
					StringBuilder sb = this.clientConnection.HasSniffers ? new StringBuilder() : null;

					await w.Lock();
					try
					{
						w.Reset(sb);
						w.WriteHeader(":status", this.statusCode.ToString(), IndexMode.Indexed, true);
						w.WriteHeader("date", CommonTypes.EncodeRfc822(this.date), IndexMode.NotIndexed, true);

						if (this.expires.HasValue)
							w.WriteHeader("expires", CommonTypes.EncodeRfc822(this.expires.Value), IndexMode.NotIndexed, true);

						if (!string.IsNullOrEmpty(this.server))
							w.WriteHeader("server", this.server, IndexMode.Indexed, true);
						else if (!string.IsNullOrEmpty(this.httpServer?.Name))
							w.WriteHeader("server", this.httpServer.Name, IndexMode.Indexed, true);

						if (!string.IsNullOrEmpty(this.contentLanguage))
							w.WriteHeader("content-language", this.contentLanguage, IndexMode.Indexed, true);

						if (!string.IsNullOrEmpty(this.contentType))
						{
							string s = this.contentType;

							if (!this.encodingUsed && s.StartsWith("text/"))
								this.encodingUsed = true;

							if (this.encodingUsed && !s.Contains("charset="))
							{
								s += "; charset=" + this.encoding.WebName;
								this.txText = true;
							}
							else if (this.clientConnection?.HasSniffers ?? false)
								this.txText = HttpClientConnection.IsSniffableTextType(s);
							else
								this.txText = false;

							w.WriteHeader("content-type", s, IndexMode.Indexed, true);
							w.WriteHeader("x-content-type-options", "nosniff", IndexMode.Indexed, true);
						}

						if ((ExpectContent || this.contentLength.HasValue) &&
							((this.statusCode >= 100 && this.statusCode <= 199) || this.statusCode == 204 || this.statusCode == 304))
						{
							throw new Exception("Content not allowed for status codes " + this.statusCode.ToString());

							// When message bodies are required:
							// http://stackoverflow.com/questions/299628/is-an-entity-body-allowed-for-an-http-delete-request
						}

						IContentEncoding ContentEncoding = this.httpRequest?.Header?.AcceptEncoding?.TryGetBestContentEncoder(this.contentLength.HasValue ? this.eTag : null);

						if (ContentEncoding is null && this.txText)
							Http2TransferEncoding.DataEncoding = this.encoding;

						if (this.contentLength.HasValue && ContentEncoding is null)
							w.WriteHeader("content-length", this.contentLength.Value.ToString(), IndexMode.NotIndexed, true);
						else if (ExpectContent)
						{
							if (!(ContentEncoding is null) &&
								(!this.contentLength.HasValue || (this.contentLength.Value >= 128 && this.contentLength.Value < 128 * 1024 * 1024)) &&
								(string.IsNullOrEmpty(this.contentType) ||
								!(this.contentType.StartsWith("image/") ||
								this.contentType.StartsWith("audio/") ||
								this.contentType.StartsWith("video/") ||
								this.contentType == BinaryCodec.DefaultContentType)))
							{
								w.WriteHeader("content-encoding", ContentEncoding.Label, IndexMode.Indexed, true);

								FileInfo PrecompressedFile = ContentEncoding.TryGetPrecompressedFile(this.contentLength.HasValue ? this.eTag : null);

								if (PrecompressedFile?.Exists ?? false)
								{
									Http2TransferEncoding.DataEncoding = null;

									w.WriteHeader("content-length", PrecompressedFile.Length.ToString(), IndexMode.NotIndexed, true);
									Http2TransferEncoding.ContentLength = PrecompressedFile.Length;

									this.transferEncoding = new PrecompressedFileReturner(PrecompressedFile, this.transferEncoding);
								}
								else
								{
									this.transferEncoding = ContentEncoding.GetEncoder(this.transferEncoding, this.contentLength,
										this.contentLength.HasValue ? this.eTag : null);
								}
							}
						}
						else
						{
							if ((this.statusCode < 100 || this.statusCode > 199) && this.statusCode != 204 && this.statusCode != 304)
								w.WriteHeader("content-length", "0", IndexMode.Indexed, true);
						}

						if (!(this.challenges is null))
						{
							foreach (string Challenge in this.challenges)
								w.WriteHeader("www-authenticate", Challenge, IndexMode.NotIndexed, true);
						}

						if (!string.IsNullOrEmpty(this.eTag))
							w.WriteHeader("etag", this.eTag, IndexMode.NotIndexed, true);

						if (!(this.customHeaders is null))
						{
							foreach (KeyValuePair<string, string> P in this.customHeaders)
								w.WriteHeaderCheckCookie(P.Key.ToLower(), P.Value, IndexMode.NotIndexed, true);
						}

						if (!(this.cookies is null))
						{
							foreach (Cookie Cookie in this.cookies)
								w.WriteHeader("set-cookie", Cookie.ToString(), IndexMode.NotIndexed, true);
						}

						byte[] HeaderBin = w.ToArray();

						if (this.clientConnection.Disposed)
							return;

						if (!await this.http2Stream.WriteHeaders(HeaderBin, ExpectContent))
							return;

						this.clientConnection.Server.DataTransmitted(HeaderBin.Length);

						if (!(sb is null))
							await this.clientConnection.TransmitText(sb.ToString());
					}
					finally
					{
						w.Release();
					}
				}
			}
		}

		/// <summary>
		/// Returns an object to the client. This method can only be called once per response, and only as the only method that returns a response
		/// to the client.
		/// </summary>
		/// <param name="Object">Object to return. Object will be encoded using Internet Content encoders, as defined in <see cref="Waher.Content"/>.</param>
		public async Task Return(object Object)
		{
			if (Object is FileReference Ref)
				await this.Return(Ref);
			else
			{
				EncodingResult Result = await this.TryEncode(Object);

				if (Result is null)
				{
					this.statusCode = NotAcceptableException.Code;
					this.statusMessage = NotAcceptableException.StatusMessage;
				}
				else
				{
					this.ContentType = Result.ContentType;
					this.ContentLength = Result.Data.Length;

					await this.Write(Result.Data);
				}

				await this.SendResponse();
			}
		}

		/// <summary>
		/// Returns an encoded object to the client. This method can only be called once per response, and only as the only method 
		/// that returns a response to the client.
		/// </summary>
		public async Task Return(string ContentType, byte[] Data)
		{
			this.ContentType = ContentType;
			this.ContentLength = Data.Length;

			await this.Write(Data);
			await this.SendResponse();
		}

		/// <summary>
		/// Returns a file.
		/// </summary>
		/// <param name="FileRef">File reference.</param>
		public async Task Return(FileReference FileRef)
		{
			using FileStream f = File.OpenRead(FileRef.FileName);
			long Pos = 0;
			long Len = f.Length;
			int BufSize = (int)Math.Min(Len, 65536 * 4);
			byte[] Buf = new byte[BufSize];
			int c;

			this.contentType = FileRef.ContentType;
			this.ContentLength = Len;

			while (Pos < Len)
			{
				c = (int)Math.Min(BufSize, Len - Pos);
				await f.ReadAllAsync(Buf, 0, c);
				await this.Write(Buf, 0, c);
				Pos += c;
			}

			await this.SendResponse();
		}

		private class EncodingResult
		{
			public byte[] Data;
			public string ContentType;
		}

		private async Task<EncodingResult> TryEncode(object Object)
		{
			HttpFieldAccept Accept = this.httpRequest?.Header?.Accept;

			if (Accept is null)
			{
				KeyValuePair<byte[], string> P = await InternetContent.EncodeAsync(Object, this.encoding);
				this.encodingUsed |= P.Value.StartsWith("text/");

				return new EncodingResult()
				{
					Data = P.Key,
					ContentType = P.Value
				};
			}
			else
			{
				string ContentType = null;
				byte[] Data = null;

				foreach (AcceptRecord Rec in Accept.Records)
				{
					switch (Rec.Detail)
					{
						case 0: // Wildcard
							KeyValuePair<byte[], string> P = await InternetContent.EncodeAsync(Object, this.encoding);
							this.encodingUsed |= P.Value.StartsWith("text/");
							Data = P.Key;
							ContentType = P.Value;
							break;

						case 1: // Top Type only
							IContentEncoder Best = null;
							double BestQuality = 0;
							string BestContentType = null;

							foreach (IContentEncoder Encoder2 in InternetContent.Encoders)
							{
								foreach (string ContentType2 in Encoder2.ContentTypes)
								{
									if (Rec.IsAcceptable(ContentType2, out double Quality, out AcceptanceLevel Acceptance))
									{
										if ((Best is null || Quality > BestQuality) && Encoder2.Encodes(Object, out Grade Grade, ContentType2))
										{
											Best = Encoder2;
											BestQuality = Quality;
											BestContentType = ContentType2;
										}
									}
								}
							}

							if (!(Best is null))
							{
								P = await Best.EncodeAsync(Object, this.encoding, ContentType, BestContentType);
								this.encodingUsed |= P.Value.StartsWith("text/");
								Data = P.Key;
								ContentType = P.Value;
							}
							break;

						case 2: // Top & Sub Type
						case 3: // Top & Sub Type, and parameters
							if (InternetContent.Encodes(Object, out Grade Grade2, out IContentEncoder Encoder, Rec.Item))
							{
								P = await Encoder.EncodeAsync(Object, this.encoding, ContentType, Rec.Item);
								this.encodingUsed |= P.Value.StartsWith("text/");
								Data = P.Key;
								ContentType = P.Value;
							}
							break;
					}

					if (!(Data is null))
					{
						return new EncodingResult()
						{
							Data = Data,
							ContentType = ContentType
						};
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Returns binary data in the response.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		public async Task Write(byte[] Data)
		{
			DateTime TP;

			if (this.transferEncoding is null)
				await this.StartSendResponse(true);

			await this.transferEncoding.EncodeAsync(Data, 0, Data.Length);

			if (!(this.httpServer is null) && ((TP = DateTime.Now) - this.lastPing).TotalSeconds >= 1)
			{
				this.lastPing = TP;
				this.httpServer?.PingRequest(this.httpRequest);
			}
		}

		internal async Task WriteRawAsync(byte[] Data)
		{
			if (!(this.responseStream is null))
			{
				TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

				await this.responseStream.SendAsync(Data, 0, Data.Length, (Sender, e) =>
				{
					Result.TrySetResult(true);
					return Task.CompletedTask;
				}, null);

				await Result.Task;
			}
		}

		/// <summary>
		/// Returns binary data in the response.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <param name="Offset">Offset into <paramref name="Data"/>.</param>
		/// <param name="Count">Number of bytes to return.</param>
		public async Task Write(byte[] Data, int Offset, int Count)
		{
			DateTime TP;

			if (this.transferEncoding is null)
				await this.StartSendResponse(true);

			await this.transferEncoding.EncodeAsync(Data, Offset, Count);

			if (!(this.httpServer is null) && ((TP = DateTime.Now) - this.lastPing).TotalSeconds >= 1)
			{
				this.lastPing = TP;
				this.httpServer.PingRequest(this.httpRequest);
			}
		}

		/// <summary>
		/// Writes a character to the stream.
		/// </summary>
		/// <param name="value">The character to write to the text stream.</param>
		/// <exception cref="IOException">An I/O error occurs.</exception>
		/// <exception cref="ObjectDisposedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer is full, 
		/// and current writer is closed.</exception>
		/// <exception cref="NotSupportedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the System.IO.StreamWriter 
		/// is at the end the stream.</exception>
		public Task Write(char value)
		{
			this.encodingUsed = true;
			return this.Write(this.encoding.GetBytes(new char[] { value }));
		}

		/// <summary>
		/// Writes a character array to the stream.
		/// </summary>
		/// <param name="buffer">A character array containing the data to write. If buffer is null, nothing is written.</param>
		/// <exception cref="IOException">An I/O error occurs.</exception>
		/// <exception cref="ObjectDisposedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and current writer is closed.</exception>
		/// <exception cref="NotSupportedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the System.IO.StreamWriter 
		/// is at the end the stream.</exception>
		public Task Write(char[] buffer)
		{
			this.encodingUsed = true;
			return this.Write(this.encoding.GetBytes(buffer));
		}

		/// <summary>
		/// Writes a string to the stream.
		/// </summary>
		/// <param name="value">The string to write to the stream. If value is null, nothing is written.</param>
		/// <exception cref="ObjectDisposedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and current writer is closed.</exception>
		/// <exception cref="NotSupportedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the System.IO.StreamWriter 
		/// is at the end the stream.</exception>
		/// <exception cref="IOException">An I/O error occurs.</exception>
		public Task Write(string value)
		{
			this.encodingUsed = true;
			return this.Write(this.encoding.GetBytes(value));
		}

		/// <summary>
		/// Writes a subarray of characters to the stream.
		/// </summary>
		/// <param name="buffer">A character array containing the data to write.</param>
		/// <param name="index">The index into buffer at which to begin writing.</param>
		/// <param name="count">The number of characters to read from buffer.</param>
		/// <exception cref="ArgumentNullException">buffer is null.</exception>
		/// <exception cref="ArgumentException">The buffer length minus index is less than count.</exception>
		/// <exception cref="ArgumentOutOfRangeException">index or count is negative.</exception>
		/// <exception cref="IOException">An I/O error occurs.</exception>
		/// <exception cref="ObjectDisposedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and current writer is closed.</exception>
		/// <exception cref="NotSupportedException">System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
		/// is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the System.IO.StreamWriter 
		/// is at the end the stream.</exception>
		public Task Write(char[] buffer, int index, int count)
		{
			this.encodingUsed = true;
			return this.Write(this.encoding.GetBytes(buffer, index, count));
		}

		/// <summary>
		/// Sets a cookie in the response.
		/// </summary>
		/// <param name="Cookie">Cookie.</param>
		public void SetCookie(Cookie Cookie)
		{
			if (this.cookies is null)
				this.cookies = new LinkedList<Cookie>();
			else
			{
				LinkedListNode<Cookie> Loop = this.cookies.First;
				while (!(Loop is null))
				{
					if (Loop.Value.Name == Cookie.Name)
					{
						Loop.Value = Cookie;
						return;
					}

					Loop = Loop.Next;
				}
			}

			this.cookies.AddLast(Cookie);
		}

		/// <summary>
		/// Sets the response stream of the response. Can only be set, if not set before.
		/// </summary>
		/// <param name="ResponseStream">New response stream.</param>
		/// <exception cref="Exception">If a response stream has already been set.</exception>
		public void SetResponseStream(Stream ResponseStream)
		{
			if (!(this.responseStream is null))
				throw new Exception("Response stream already set.");

			this.responseStream = new BinaryOutputStream(ResponseStream);
		}

		/// <summary>
		/// Event raised when the response has been sent.
		/// </summary>
		public EventHandlerAsync OnResponseSent = null;

		#region TextWriter analogies

		/// <summary>
		/// Writes a value to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public Task Write(ulong value)
		{
			return this.Write(value.ToString());
		}

		/// <summary>
		/// Writes a value to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public Task Write(uint value)
		{
			return this.Write(value.ToString());
		}

		/// <summary>
		/// Writes a formatted string to the stream.
		/// </summary>
		/// <param name="format">Format</param>
		/// <param name="arg">Arguments</param>
		public Task Write(string format, params object[] arg)
		{
			return this.Write(string.Format(format, arg));
		}

		/// <summary>
		/// Writes a formatted string to the stream.
		/// </summary>
		/// <param name="format">Format</param>
		/// <param name="arg0">First argument</param>
		public Task Write(string format, object arg0)
		{
			return this.Write(string.Format(format, arg0));
		}

		/// <summary>
		/// Writes a formatted string to the stream.
		/// </summary>
		/// <param name="format">Format</param>
		/// <param name="arg0">First argument</param>
		/// <param name="arg1">Second argument</param>
		public Task Write(string format, object arg0, object arg1)
		{
			return this.Write(string.Format(format, arg0, arg1));
		}

		/// <summary>
		/// Writes a formatted string to the stream.
		/// </summary>
		/// <param name="format">Format</param>
		/// <param name="arg0">First argument</param>
		/// <param name="arg1">Second argument</param>
		/// <param name="arg2">Third argument</param>
		public Task Write(string format, object arg0, object arg1, object arg2)
		{
			return this.Write(string.Format(format, arg0, arg1, arg2));
		}

		/// <summary>
		/// Writes a value to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public Task Write(float value)
		{
			return this.Write(value.ToString());
		}

		/// <summary>
		/// Writes a value to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public Task Write(long value)
		{
			return this.Write(value.ToString());
		}

		/// <summary>
		/// Writes a value to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public Task Write(int value)
		{
			return this.Write(value.ToString());
		}

		/// <summary>
		/// Writes a value to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public Task Write(double value)
		{
			return this.Write(value.ToString());
		}

		/// <summary>
		/// Writes a value to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public Task Write(decimal value)
		{
			return this.Write(value.ToString());
		}

		/// <summary>
		/// Writes a value to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public Task Write(bool value)
		{
			return this.Write(value.ToString());
		}

		/// <summary>
		/// Writes a value to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public Task Write(object value)
		{
			if (value is byte[] Bin)
				return this.Write(Bin);
			else if (value is string s)
				return this.Write(s);
			else if (value is char ch)
				return this.Write(ch);
			else
				return this.Write(value.ToString());
		}

		/// <summary>
		/// Writes a value to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		[Obsolete("Use Write() instead.")]
		public Task WriteAsync(string value)
		{
			return this.Write(value);
		}

		/// <summary>
		/// Writes characters to the stream.
		/// </summary>
		/// <param name="buffer">Character buffer</param>
		/// <param name="index">Index to first character</param>
		/// <param name="count">Number of characters to output</param>
		[Obsolete("Use Write() instead.")]
		public Task WriteAsync(char[] buffer, int index, int count)
		{
			return this.Write(buffer, index, count);
		}

		/// <summary>
		/// Writes characters to the stream.
		/// </summary>
		/// <param name="buffer">Character buffer</param>
		[Obsolete("Use Write() instead.")]
		public Task WriteAsync(char[] buffer)
		{
			return this.Write(buffer);
		}

		/// <summary>
		/// Writes a character to the stream.
		/// </summary>
		/// <param name="value">Character</param>
		[Obsolete("Use Write() instead.")]
		public Task WriteAsync(char value)
		{
			return this.Write(value);
		}

		/// <summary>
		/// Writes a new line character sequence (CRLF).
		/// </summary>
		public Task WriteLine()
		{
			return this.Write(CRLF);
		}

		private static readonly byte[] CRLF = new byte[] { (byte)'\r', (byte)'\n' };

		/// <summary>
		/// Writes a value, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public async Task WriteLine(ulong value)
		{
			await this.Write(value);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a value, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public async Task WriteLine(uint value)
		{
			await this.Write(value);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a formatted string, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="format">Format</param>
		/// <param name="arg">Arguments</param>
		public async Task WriteLine(string format, params object[] arg)
		{
			await this.Write(format, arg);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a formatted string, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="format">Format</param>
		/// <param name="arg0">First argument</param>
		public async Task WriteLine(string format, object arg0)
		{
			await this.Write(format, arg0);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a formatted string, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="format">Format</param>
		/// <param name="arg0">First argument</param>
		/// <param name="arg1">Sceond argument</param>
		public async Task WriteLine(string format, object arg0, object arg1)
		{
			await this.Write(format, arg0, arg1);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a formatted string, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="format">Format</param>
		/// <param name="arg0">First argument</param>
		/// <param name="arg1">Sceond argument</param>
		/// <param name="arg2">Third argument</param>
		public async Task WriteLine(string format, object arg0, object arg1, object arg2)
		{
			await this.Write(format, arg0, arg1, arg2);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a value, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public async Task WriteLine(string value)
		{
			await this.Write(value);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a value, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public async Task WriteLine(float value)
		{
			await this.Write(value);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a value, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public async Task WriteLine(long value)
		{
			await this.Write(value);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a value, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public async Task WriteLine(int value)
		{
			await this.Write(value);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a value, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public async Task WriteLine(double value)
		{
			await this.Write(value);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a value, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public async Task WriteLine(decimal value)
		{
			await this.Write(value);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a sequence of characters, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="buffer">Characters</param>
		/// <param name="index">Index to the first character to write</param>
		/// <param name="count">Number of characters to write</param>
		public async Task WriteLine(char[] buffer, int index, int count)
		{
			await this.Write(buffer, index, count);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a sequence of characters, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="buffer">Characters</param>
		public async Task WriteLine(char[] buffer)
		{
			await this.Write(buffer);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a character, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Character</param>
		public async Task WriteLine(char value)
		{
			await this.Write(value);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a value, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public async Task WriteLine(object value)
		{
			await this.Write(value);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a value, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		public async Task WriteLine(bool value)
		{
			await this.Write(value);
			await this.WriteLine();
		}

		/// <summary>
		/// Writes a new line character sequence (CRLF).
		/// </summary>
		[Obsolete("Use WriteLine() instead.")]
		public Task WriteLineAsync()
		{
			return this.WriteLine();
		}

		/// <summary>
		/// Writes a character, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Character</param>
		[Obsolete("Use WriteLine() instead.")]
		public Task WriteLineAsync(char value)
		{
			return this.WriteLine(value);
		}

		/// <summary>
		/// Writes a sequence of characters, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="buffer">Characters</param>
		[Obsolete("Use WriteLine() instead.")]
		public Task WriteLineAsync(char[] buffer)
		{
			return this.WriteLine(buffer);
		}

		/// <summary>
		/// Writes a sequence of characters, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="buffer">Characters</param>
		/// <param name="index">Index to the first character to write</param>
		/// <param name="count">Number of characters to write</param>
		[Obsolete("Use WriteLine() instead.")]
		public Task WriteLineAsync(char[] buffer, int index, int count)
		{
			return this.WriteLine(buffer, index, count);
		}

		/// <summary>
		/// Writes a value, followed by a carriage return and linefeed, to the stream.
		/// </summary>
		/// <param name="value">Value</param>
		[Obsolete("Use WriteLine() instead.")]
		public Task WriteLineAsync(string value)
		{
			return this.WriteLine(value);
		}

		#endregion
	}
}
