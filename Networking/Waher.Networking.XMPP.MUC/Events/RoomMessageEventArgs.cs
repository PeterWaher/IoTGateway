using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Delegate for MUC Room event handlers.
	/// </summary>
	/// <param name="Sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	public delegate Task RoomMessageEventHandler(object Sender, RoomMessageEventArgs e);

	/// <summary>
	/// Message from a MUC room.
	/// </summary>
	public class RoomMessageEventArgs : MessageEventArgs
	{
		private readonly string roomId;
		private readonly string domain;

		/// <summary>
		/// Message from a MUC room.
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		public RoomMessageEventArgs(MessageEventArgs e, string RoomId, string Domain)
			: base(e)
		{
			this.roomId = RoomId;
			this.domain = Domain;
		}

		/// <summary>
		/// Room ID
		/// </summary>
		public string RoomId => this.roomId;

		/// <summary>
		/// Domain hosting the room.
		/// </summary>
		public string Domain => this.domain;
	}
}
