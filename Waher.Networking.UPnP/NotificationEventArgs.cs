using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// UPnP Notification event handler.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void NotificationEventHandler(UPnPClient Sender, NotificationEventArgs e);

	/// <summary>
	/// Contains information about the location of a device on the network.
	/// </summary>
	public class NotificationEventArgs
	{
		private UPnPClient client;
		private UPnPHeaders headers;
		private IPEndPoint localEndPoint;
		private IPEndPoint remoteEndPoint;

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
		public UPnPClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// UPnP Headers
		/// </summary>
		public UPnPHeaders Headers
		{
			get { return this.headers; }
		}

		/// <summary>
		/// Local End Point
		/// </summary>
		public IPEndPoint LocalEndPoint
		{
			get { return this.localEndPoint; }
		}

		/// <summary>
		/// Remote End Point
		/// </summary>
		public IPEndPoint RemoteEndPoint
		{
			get { return this.remoteEndPoint; }
		}

	}
}
