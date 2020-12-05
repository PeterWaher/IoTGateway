using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Delegate for User Presence events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task UserPresenceEventHandlerAsync(object Sender, UserPresenceEventArgs e);

	/// <summary>
	/// Event arguments for user presence events.
	/// </summary>
	public class UserPresenceEventArgs : PresenceEventArgs
	{
		private readonly Affiliation affiliation;
		private readonly Role role;
		private readonly MucStatus[] status;
		private readonly string roomId;
		private readonly string domain;
		private readonly string userNick;
		private readonly string fullJid;

		/// <summary>
		/// Event arguments for user presence events.
		/// </summary>
		/// <param name="e">Presence event arguments</param>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="UserNick">Nick-Name of user sending presence.</param>
		/// <param name="Affiliation">User affiliation.</param>
		/// <param name="Role">User role.</param>
		/// <param name="FullJid">Full JID, if privileges allow.</param>
		/// <param name="Status">Status codes.</param>
		public UserPresenceEventArgs(PresenceEventArgs e, string RoomId, string Domain, 
			string UserNick, Affiliation Affiliation, Role Role, string FullJid,
			params MucStatus[] Status)
			: base(e)
		{
			this.roomId = RoomId;
			this.domain = Domain;
			this.userNick = UserNick;
			this.affiliation = Affiliation;
			this.role = Role;
			this.fullJid = FullJid;
			this.status = Status;
		}

		/// <summary>
		/// Room ID
		/// </summary>
		public string RoomId => this.roomId;

		/// <summary>
		/// Domain hosting the room.
		/// </summary>
		public string Domain => this.domain;

		/// <summary>
		/// Nick-Name of user sending presence.
		/// </summary>
		public override string NickName => this.userNick;

		/// <summary>
		/// Full JID, if privileges allow (null otherwise).
		/// </summary>
		public string FullJid => this.fullJid;

		/// <summary>
		/// User Affiliation in room.
		/// </summary>
		public Affiliation Affiliation => this.affiliation;

		/// <summary>
		/// User role in room.
		/// </summary>
		public Role Role => this.role;

		/// <summary>
		/// Any status codes informing about changes to status.
		/// </summary>
		public MucStatus[] MucStatus => this.status;
	}
}
