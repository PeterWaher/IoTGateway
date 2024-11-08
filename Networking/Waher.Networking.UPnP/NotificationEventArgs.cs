using System.Net;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Contains information about the location of a device on the network.
	/// </summary>
	public class NotificationEventArgs
	{
		private readonly UPnPClient client;
		private readonly UPnPHeaders headers;
		private readonly IPEndPoint localEndPoint;
		private readonly IPEndPoint remoteEndPoint;

		/// <summary>
		/// Contains information about the location of a device on the network.
		/// </summary>
		/// <param name="Client">UPnP Client</param>
		/// <param name="Headers">All headers in notification.</param>
		/// <param name="LocalEndPoint">Local End Point.</param>
		/// <param name="RemoteEndPoint">Remote End Point.</param>
		internal NotificationEventArgs(UPnPClient Client, UPnPHeaders Headers, IPEndPoint LocalEndPoint, IPEndPoint RemoteEndPoint)
		{
			this.client = Client;
			this.headers = Headers;
			this.localEndPoint = LocalEndPoint;
			this.remoteEndPoint = RemoteEndPoint;
		}

		/// <summary>
		/// UPnP Client
		/// </summary>
		public UPnPClient Client => this.client;

		/// <summary>
		/// UPnP Headers
		/// </summary>
		public UPnPHeaders Headers => this.headers;

		/// <summary>
		/// Local End Point
		/// </summary>
		public IPEndPoint LocalEndPoint => this.localEndPoint;

		/// <summary>
		/// Remote End Point
		/// </summary>
		public IPEndPoint RemoteEndPoint => this.remoteEndPoint;
	}
}
