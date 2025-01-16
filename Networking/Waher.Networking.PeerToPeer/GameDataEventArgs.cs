using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.MQTT;

namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// Event arguments for game data events.
	/// </summary>
	public class GameDataEventArgs : EventArgs
	{
		private readonly Player fromPlayer;
		private readonly PeerConnection connection;
		private readonly byte[] packet;
		private readonly BinaryInput data;

		internal GameDataEventArgs(Player FromPlayer, PeerConnection Connection, byte[] Packet)
		{
			this.fromPlayer = FromPlayer;
			this.connection = Connection;
			this.packet = Packet;
			this.data = new BinaryInput(Packet);
		}

		/// <summary>
		/// Game data received from this player.
		/// </summary>
		public Player FromPlayer => this.fromPlayer;

		/// <summary>
		/// Game data received over this connection.
		/// </summary>
		public PeerConnection Connection => this.connection;

		/// <summary>
		/// Binary game data packet received.
		/// </summary>
		public byte[] Packet => this.packet;

		/// <summary>
		/// Game data received.
		/// </summary>
		public BinaryInput Data => this.data;
	}
}
