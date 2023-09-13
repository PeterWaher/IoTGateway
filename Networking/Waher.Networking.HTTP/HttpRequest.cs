using System;
using System.IO;
#if !WINDOWS_UWP
using System.Security.Cryptography.X509Certificates;
#endif
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Script;
using Waher.Security;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Represents an HTTP request.
	/// </summary>
	public class HttpRequest : IDisposable
	{
		private readonly HttpRequestHeader header;
		private Stream dataStream;
		private readonly HttpServer server;
		private readonly string remoteEndPoint;
		private IUser user = null;
		private Variables session = null;
		private string subPath = string.Empty;
		private HttpResource resource = null;
		private HttpResponse response = null;
		private Guid? requestId = null;
		internal HttpClientConnection clientConnection = null;
		internal bool tempSession = false;

		/// <summary>
		/// Represents an HTTP request.
		/// </summary>
		/// <param name="Server">HTTP Server receiving the request.</param>
		/// <param name="Header">HTTP Request header.</param>
		/// <param name="Data">Stream to data content, if available, or null, if request does not have a message body.</param>
		/// <param name="RemoteEndPoint">Remote end-point.</param>
		public HttpRequest(HttpServer Server, HttpRequestHeader Header, Stream Data, string RemoteEndPoint)
		{
			this.server = Server;
			this.header = Header;
			this.dataStream = Data;
			this.remoteEndPoint = RemoteEndPoint;

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
		/// Decodes data sent in request.
		/// </summary>
		/// <returns>Decoded data.</returns>
		[Obsolete("Use DecodeDataAsync() instead, for better performance processing asynchronous elements in parallel environments.")]
		public object DecodeData()
		{
			return this.DecodeDataAsync().Result;
		}

		/// <summary>
		/// Decodes data sent in request.
		/// </summary>
		/// <returns>Decoded data.</returns>
		public async Task<object> DecodeDataAsync()
		{
			byte[] Data = await this.ReadDataAsync();
			if (Data is null)
				return null;

			HttpFieldContentType ContentType = this.header.ContentType;
			if (ContentType is null)
				return Data;

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

			long l = this.dataStream.Length;
			if (l > int.MaxValue)
				throw new OutOfMemoryException("Data object too large for in-memory decoding.");

			int Len = (int)l;
			byte[] Data = new byte[Len];
			this.dataStream.Position = 0;
			await this.dataStream.ReadAsync(Data, 0, Len);

			return Data;
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
		public Variables Session
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
		/// HTTP Response object, if one has been assigned to the request.
		/// </summary>
		public HttpResponse Response
		{
			get => this.response;
			internal set => this.response = value;
		}

#if WINDOWS_UWP
		/// <summary>
		/// If the connection is encrypted or not.
		/// </summary>
		public bool Encrypted => false;

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
		public bool Encrypted => this.clientConnection?.Encrypted ?? false;

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
		/// Disposes of the request.
		/// </summary>
		public void Dispose()
		{
			this.dataStream?.Dispose();
			this.dataStream = null;
		}
	}
}
