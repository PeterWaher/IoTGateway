using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Waher.Content;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Script;
using Waher.Security;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Represents a HTTP request.
	/// </summary>
	public class HttpRequest : IDisposable
	{
		private HttpRequestHeader header;
		private Stream dataStream;
		private Stream responseStream;
		private IUser user = null;
		private Variables session = null;
		private string subPath = string.Empty;
		private long dataLength;

		/// <summary>
		/// Represents a HTTP request.
		/// </summary>
		/// <param name="Header">HTTP Request header.</param>
		/// <param name="Data">Stream to data content, if available, or null, if request does not have a message body.</param>
		/// <param name="ResponseStream">Response stream.</param>
		public HttpRequest(HttpRequestHeader Header, Stream Data, Stream ResponseStream)
		{
			this.header = Header;
			this.dataStream = Data;
			this.responseStream = ResponseStream;

			if (this.dataStream == null)
				this.dataLength = 0;
			else
			{
				this.dataLength = this.dataStream.Position;
				this.dataStream.Position = 0;
			}
		}

		/// <summary>
		/// If the request has data.
		/// </summary>
		public bool HasData
		{
			get { return this.dataStream != null; }
		}

		/// <summary>
		/// Decodes data sent in request.
		/// </summary>
		/// <returns>Decoded data.</returns>
		public object DecodeData()
		{
			if (this.dataStream == null)
				return null;

			long l = this.dataStream.Length;
			if (l > int.MaxValue)
				throw new OutOfMemoryException("Data object too large for in-memory decoding.");

			int Len = (int)l;
			byte[] Data = new byte[Len];
			this.dataStream.Position = 0;
			this.dataStream.Read(Data, 0, Len);

			HttpFieldContentType ContentType = this.header.ContentType;
			if (ContentType == null)
				return Data;

			return InternetContent.Decode(ContentType.Type, Data, ContentType.Encoding, ContentType.Fields);
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
			internal set { this.subPath = value; }
		}

		/// <summary>
		/// Authenticated user, if available, or null if not available.
		/// </summary>
		public IUser User
		{
			get { return this.user; }
			internal set { this.user = value; }
		}

        /// <summary>
        /// Contains session states, if the resource requires sessions, or null otherwise.
        /// </summary>
        public Variables Session
        {
            get { return this.session; }
            internal set { this.session = value; }
        }

		/// <summary>
		/// Disposes of the request.
		/// </summary>
		public void Dispose()
		{
			if (this.dataStream != null)
			{
				this.dataStream.Dispose();
				this.dataStream = null;

				this.responseStream = null;
				this.header = null;
			}
		}
	}
}
