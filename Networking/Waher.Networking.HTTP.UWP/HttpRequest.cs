using System;
using System.IO;
#if !WINDOWS_UWP
using System.Security.Cryptography.X509Certificates;
#endif
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Binary;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.HTTP.HTTP2;
using Waher.Runtime.IO;
using Waher.Security;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Represents an HTTP request.
	/// </summary>
	public class HttpRequest : IDisposable, IHostReference
	{
		private readonly HttpRequestHeader header;
		private Stream dataStream;
		private readonly HttpServer server;
		private readonly string remoteEndPoint;
		private readonly string localEndPoint;
		private readonly Http2Stream http2Stream;
		private IUser user = null;
		private SessionVariables session = null;
		private string subPath = string.Empty;
		private HttpResource resource = null;
		private HttpResponse response = null;
		private Guid? requestId = null;
		internal HttpClientConnection clientConnection = null;
		internal bool tempSession = false;
		internal bool defaultEncrypted = false;

		/// <summary>
		/// Represents an HTTP request.
		/// </summary>
		/// <param name="Server">HTTP Server receiving the request.</param>
		/// <param name="Header">HTTP Request header.</param>
		/// <param name="Data">Stream to data content, if available, or null, if request does not have a message body.</param>
		/// <param name="RemoteEndPoint">Remote end-point.</param>
		/// <param name="LocalEndPoint">Local end-point.</param>
		public HttpRequest(HttpServer Server, HttpRequestHeader Header, Stream Data, string RemoteEndPoint,
			string LocalEndPoint)
			: this(Server, Header, Data, RemoteEndPoint, LocalEndPoint, false, null)
		{
		}

		/// <summary>
		/// Represents an HTTP request.
		/// </summary>
		/// <param name="Server">HTTP Server receiving the request.</param>
		/// <param name="Header">HTTP Request header.</param>
		/// <param name="Data">Stream to data content, if available, or null, if request does not have a message body.</param>
		/// <param name="RemoteEndPoint">Remote end-point.</param>
		/// <param name="LocalEndPoint">Local end-point.</param>
		/// <param name="Http2Stream">HTTP/2 stream</param>
		public HttpRequest(HttpServer Server, HttpRequestHeader Header, Stream Data, string RemoteEndPoint,
			string LocalEndPoint, Http2Stream Http2Stream)
			: this(Server, Header, Data, RemoteEndPoint, LocalEndPoint, false, Http2Stream)
		{
		}

		/// <summary>
		/// Represents an HTTP request.
		/// </summary>
		/// <param name="Server">HTTP Server receiving the request.</param>
		/// <param name="Header">HTTP Request header.</param>
		/// <param name="Data">Stream to data content, if available, or null, if request does not have a message body.</param>
		/// <param name="RemoteEndPoint">Remote end-point.</param>
		/// <param name="LocalEndPoint">Local end-point.</param>
		/// <param name="DefaultEncrypted">If underlying transport is encrypted by default.</param>
		public HttpRequest(HttpServer Server, HttpRequestHeader Header, Stream Data, string RemoteEndPoint,
			string LocalEndPoint, bool DefaultEncrypted)
			: this(Server, Header, Data, RemoteEndPoint, LocalEndPoint, DefaultEncrypted, null)
		{
		}

		/// <summary>
		/// Represents an HTTP request.
		/// </summary>
		/// <param name="Server">HTTP Server receiving the request.</param>
		/// <param name="Header">HTTP Request header.</param>
		/// <param name="Data">Stream to data content, if available, or null, if request does not have a message body.</param>
		/// <param name="RemoteEndPoint">Remote end-point.</param>
		/// <param name="LocalEndPoint">Local end-point.</param>
		/// <param name="DefaultEncrypted">If underlying transport is encrypted by default.</param>
		/// <param name="Http2Stream">HTTP/2 stream</param>
		public HttpRequest(HttpServer Server, HttpRequestHeader Header, Stream Data, string RemoteEndPoint,
			string LocalEndPoint, bool DefaultEncrypted, Http2Stream Http2Stream)
		{
			this.server = Server;
			this.header = Header;
			this.dataStream = Data;
			this.remoteEndPoint = RemoteEndPoint;
			this.localEndPoint = LocalEndPoint;
			this.defaultEncrypted = DefaultEncrypted;
			this.http2Stream = Http2Stream;

			if (!(this.dataStream is null))
				this.dataStream.Position = 0;
		}

		/// <summary>
		/// If the request has data.
		/// </summary>
		public bool HasData => !(this.dataStream is null);

		/// <summary>
		/// HTTP Server receiving the request.
		/// </summary>
		public HttpServer Server => this.server;

		/// <summary>
		/// HTTP/2 stream
		/// </summary>
		public Http2Stream Http2Stream => this.http2Stream;

		/// <summary>
		/// Decodes data sent in request.
		/// </summary>
		/// <returns>Decoded data.</returns>
		[Obsolete("Use DecodeDataAsync() instead, for better performance processing asynchronous elements in parallel environments.")]
		public ContentResponse DecodeData()
		{
			return this.DecodeDataAsync().Result;
		}

		/// <summary>
		/// Decodes data sent in request.
		/// </summary>
		/// <returns>Decoded data.</returns>
		public async Task<ContentResponse> DecodeDataAsync()
		{
			byte[] Data = await this.ReadDataAsync();
			if (Data is null)
			{
				Data = Array.Empty<byte>();
				return new ContentResponse(BinaryCodec.DefaultContentType, Data, Data);
			}

			HttpFieldContentType ContentType = this.header.ContentType;
			if (ContentType is null)
				return new ContentResponse(BinaryCodec.DefaultContentType, Data, Data);

			return await InternetContent.DecodeAsync(ContentType.Type, Data, ContentType.Encoding, ContentType.Fields,
				new Uri(this.header.GetURL(false, false)));
		}

		/// <summary>
		/// Reads posted binary data
		/// </summary>
		/// <returns>Binary data, undecoded.</returns>
		/// <exception cref="OutOfMemoryException">If posted object is too large for in-memory processing.</exception>
		public async Task<byte[]> ReadDataAsync()
		{
			if (this.dataStream is null)
				return null;

			this.dataStream.Position = 0;
			return await this.dataStream.ReadAllAsync();
		}

		/// <summary>
		/// Request header.
		/// </summary>
		public HttpRequestHeader Header => this.header;

		/// <summary>
		/// Data stream, if data is available, or null if data is not available.
		/// </summary>
		public Stream DataStream => this.dataStream;

		/// <summary>
		/// Sub-path. If a resource is found handling the request, this property contains the trailing sub-path of the full path,
		/// relative to the path of the resource object.
		/// </summary>
		public string SubPath
		{
			get => this.subPath;
			set => this.subPath = value;
		}

		/// <summary>
		/// Authenticated user, if available, or null if not available.
		/// </summary>
		public IUser User
		{
			get => this.user;
			set => this.user = value;
		}

		/// <summary>
		/// Contains session states, if the resource requires sessions, or null otherwise.
		/// </summary>
		public SessionVariables Session
		{
			get => this.session;
			set => this.session = value;
		}

		/// <summary>
		/// Resource being accessed.
		/// </summary>
		public HttpResource Resource
		{
			get => this.resource;
			set => this.resource = value;
		}

		/// <summary>
		/// ID of request.
		/// </summary>
		public Guid RequestId
		{
			get
			{
				if (!this.requestId.HasValue)
					this.requestId = Guid.NewGuid();

				return this.requestId.Value;
			}
		}

		/// <summary>
		/// Remote end-point.
		/// </summary>
		public string RemoteEndPoint => this.remoteEndPoint;

		/// <summary>
		/// Local end-point.
		/// </summary>
		public string LocalEndPoint => this.localEndPoint;

		/// <summary>
		/// HTTP Response object, if one has been assigned to the request.
		/// </summary>
		public HttpResponse Response
		{
			get => this.response;
			internal set => this.response = value;
		}

		/// <summary>
		/// Host reference. (Value of Host header, without the port number)
		/// </summary>
		public string Host
		{
			get
			{
				string s = this.header?.Host?.Value;
				if (string.IsNullOrEmpty(s))
					return s;

				return s.RemovePortNumber();
			}
		}

#if WINDOWS_UWP
		/// <summary>
		/// If the connection is encrypted or not.
		/// </summary>
		public bool Encrypted => this.defaultEncrypted;

		/// <summary>
		/// Cipher strength
		/// </summary>
		public int CipherStrength => 0;
#else
		/// <summary>
		/// Remote client certificate, if any, associated with the request.
		/// </summary>
		public X509Certificate RemoteCertificate => this.clientConnection?.Client?.RemoteCertificate;

		/// <summary>
		/// If the Remote client certificate, if any, is valid.
		/// </summary>
		public bool RemoteCertificateValid => this.clientConnection?.Client?.RemoteCertificateValid ?? false;

		/// <summary>
		/// If the connection is encrypted or not.
		/// </summary>
		public bool Encrypted => this.clientConnection?.Encrypted ?? this.defaultEncrypted;

		/// <summary>
		/// Cipher strength
		/// </summary>
		public int CipherStrength
		{
			get
			{
				BinaryTcpClient Client = this.clientConnection.Client;
				return Client is null ? 0 : Math.Min(Client.CipherStrength, Client.KeyExchangeStrength);
			}
		}
#endif
		/// <summary>
		/// Tries to get a file name for a resource, if local.
		/// </summary>
		/// <param name="Resource">Resource</param>
		/// <param name="Host">Optional host, if available.</param>
		/// <param name="FileName">File name, if resource identified as a local resource.</param>
		/// <returns>If successful in identifying a local file name for the resource.</returns>
		public bool TryGetLocalResourceFileName(string Resource, string Host, out string FileName)
		{
			if (string.IsNullOrEmpty(Host))
				Host = this.Host;

			return this.server.TryGetLocalResourceFileName(Resource, Host, out FileName);
		}

		/// <summary>
		/// Disposes of the request.
		/// </summary>
		public void Dispose()
		{
			this.dataStream?.Dispose();
			this.dataStream = null;
		}
	}
}
