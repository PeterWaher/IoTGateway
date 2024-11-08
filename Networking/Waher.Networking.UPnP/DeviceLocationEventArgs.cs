using System.Net;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Event arguments for completion events when downloading device description documents.
	/// </summary>
	public class DeviceLocationEventArgs
	{
		private readonly DeviceLocation location;
		private readonly IPEndPoint localEndPoint;
		private readonly IPEndPoint remoteEndPoint;

		internal DeviceLocationEventArgs(DeviceLocation Location, IPEndPoint LocalEndPoint, IPEndPoint RemoteEndPoint)
		{
			this.location = Location;
			this.localEndPoint = LocalEndPoint;
			this.remoteEndPoint = RemoteEndPoint;
		}

		/// <summary>
		/// Device Location information.
		/// </summary>
		public DeviceLocation Location => this.location;

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
