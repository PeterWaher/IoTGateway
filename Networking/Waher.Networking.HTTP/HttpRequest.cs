using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Represents a HTTP request.
	/// </summary>
	public class HttpRequest : IDisposable
	{
		private HttpRequestHeader header;
		private Stream dataStream;
		private NetworkStream responseStream;
		private string subPath = string.Empty;
		private long dataLength;

		/// <summary>
		/// Represents a HTTP request.
		/// </summary>
		/// <param name="Header">HTTP Request header.</param>
		/// <param name="Data">Stream to data content, if available, or null, if request does not have a message body.</param>
		/// <param name="ResponseStream">Response stream.</param>
		public HttpRequest(HttpRequestHeader Header, Stream Data, NetworkStream ResponseStream)
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
