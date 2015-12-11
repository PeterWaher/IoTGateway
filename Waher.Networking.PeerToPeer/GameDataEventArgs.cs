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
		private Player fromPlayer;
		private PeerConnection connection;
		private byte[] packet;
		private BinaryInput data;

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
		public Player FromPlayer { get { return this.fromPlayer; } }

		/// <summary>
		/// Game data received over this connection.
		/// </summary>
		public PeerConnection Connection { get { return this.connection; } }

		/// <summary>
		/// Binary game data packet received.
		/// </summary>
		public byte[] Packet { get { return this.packet; } }

		/// <summary>
		/// Game data received.
		/// </summary>
		public BinaryInput Data { get { return this.data; } }
	}
}
