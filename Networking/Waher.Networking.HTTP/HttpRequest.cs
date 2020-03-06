using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;
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
		private readonly string remoteEndPoint;
		private IUser user = null;
		private Variables session = null;
		private string subPath = string.Empty;
		private HttpResource resource = null;
		internal HttpClientConnection clientConnection = null;

		/// <summary>
		/// Represents an HTTP request.
		/// </summary>
		/// <param name="Header">HTTP Request header.</param>
		/// <param name="Data">Stream to data content, if available, or null, if request does not have a message body.</param>
		/// <param name="RemoteEndPoint">Remote end-point.</param>
		public HttpRequest(HttpRequestHeader Header, Stream Data, string RemoteEndPoint)
		{
			this.header = Header;
			this.dataStream = Data;
			this.remoteEndPoint = RemoteEndPoint;

			if (!(this.dataStream is null))
				this.dataStream.Position = 0;
		}

		/// <summary>
		/// If the request has data.
		/// </summary>
		public bool HasData
		{
			get { return !(this.dataStream is null); }
		}

		/// <summary>
		/// Decodes data sent in request.
		/// </summary>
		/// <returns>Decoded data.</returns>
		public object DecodeData()
		{
			if (this.dataStream is null)
				return null;

			long l = this.dataStream.Length;
			if (l > int.MaxValue)
				throw new OutOfMemoryException("Data object too large for in-memory decoding.");

			int Len = (int)l;
			byte[] Data = new byte[Len];
			this.dataStream.Position = 0;
			this.dataStream.Read(Data, 0, Len);

			HttpFieldContentType ContentType = this.header.ContentType;
			if (ContentType is null)
				return Data;

			return InternetContent.Decode(ContentType.Type, Data, ContentType.Encoding, ContentType.Fields,
				new Uri(this.header.GetURL(false, false)));
		}

		/// <summary>
		/// Request header.
		/// </summary>
		public HttpRequestHeader Header
		{
			get { return this.header; }
		}

		/// <summary>
		/// Data stream, if data is available, or null if data is not available.
		/// </summary>
		public Stream DataStream
		{
			get { return this.dataStream; }
		}

		/// <summary>
		/// Sub-path. If a resource is found handling the request, this property contains the trailing sub-path of the full path,
		/// relative to the path of the resource object.
		/// </summary>
		public string SubPath
		{
			get { return this.subPath; }
			set { this.subPath = value; }
		}

		/// <summary>
		/// Authenticated user, if available, or null if not available.
		/// </summary>
		public IUser User
		{
			get { return this.user; }
			set { this.user = value; }
		}

		/// <summary>
		/// Contains session states, if the resource requires sessions, or null otherwise.
		/// </summary>
		public Variables Session
		{
			get { return this.session; }
			set { this.session = value; }
		}

		/// <summary>
		/// Resource being accessed.
		/// </summary>
		public HttpResource Resource
		{
			get { return this.resource; }
			set { this.resource = value; }
		}

		/// <summary>
		/// Remote end-point.
		/// </summary>
		public string RemoteEndPoint
		{
			get { return this.remoteEndPoint; }
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
