using System;
using Waher.Networking.Sniffers;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Event arguments for customizing sniffers based on remote endpoint.
	/// </summary>
	public class CustomSniffersEventArgs : EventArgs
	{
		private readonly string remoteEndplint;
		private ISniffer[] sniffers;

		/// <summary>
		/// Event arguments for customizing sniffers based on remote endpoint.
		/// </summary>
		public CustomSniffersEventArgs(string RemotEndpoint, ISniffer[] Sniffers)
		{
			this.remoteEndplint = RemotEndpoint;
			this.sniffers = Sniffers;
		}

		/// <summary>
		/// Remote Endpoint.
		/// </summary>
		public string RemoteEndpoint => this.remoteEndplint;

		/// <summary>
		/// Sniffers to use for the connection.
		/// </summary>
		public ISniffer[] Sniffers
		{
			get => this.sniffers;
			set => this.sniffers = value;
		}
	}
}
