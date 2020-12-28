using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Delegate for MUC Room declined event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task RoomDeclinedMessageEventHandler(object Sender, RoomDeclinedMessageEventArgs e);

	/// <summary>
	/// Message from a MUC room containing a declined invitation.
	/// </summary>
	public class RoomDeclinedMessageEventArgs : RoomMessageEventArgs
	{
		private readonly string declinedFrom;
		private readonly string reason;

		/// <summary>
		/// Message from a MUC room containing a declined invitation.
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="DeclinedFrom">JID of entity sending the declined.</param>
		/// <param name="Reason">Reason for declined.</param>
		public RoomDeclinedMessageEventArgs(MessageEventArgs e, string RoomId, string Domain, string DeclinedFrom, string Reason)
			: base(e, RoomId, Domain)
		{
			this.declinedFrom = DeclinedFrom;
			this.reason = Reason;
		}

		/// <summary>
		/// JID of entity sending the declined invitation.
		/// </summary>
		public string DeclinedFrom => this.declinedFrom;

		/// <summary>
		/// Reason for declining the invitation.
		/// </summary>
		public string Reason => this.reason;
	}
}
