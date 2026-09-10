using System;
using Waher.Networking.MQTT;

namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// Event arguments for peer data events.
	/// </summary>
	public class PeerDataEventArgs : EventArgs
	{
		private readonly Peer fromPeer;
		private readonly PeerConnection connection;
		private readonly byte[] packet;
		private readonly BinaryInput data;

		internal PeerDataEventArgs(Peer FromPeer, PeerConnection Connection, byte[] Packet)
		{
			this.fromPeer = FromPeer;
			this.connection = Connection;
			this.packet = Packet;
			this.data = new BinaryInput(Packet);
		}

		/// <summary>
		/// Peer data received from this peer.
		/// </summary>
		public Peer FromPeer => this.fromPeer;

		/// <summary>
		/// Peer data received over this connection.
		/// </summary>
		public PeerConnection Connection => this.connection;

		/// <summary>
		/// Binary peer data packet received.
		/// </summary>
		public byte[] Packet => this.packet;

		/// <summary>
		/// Peer data received.
		/// </summary>
		public BinaryInput Data => this.data;
	}
}
