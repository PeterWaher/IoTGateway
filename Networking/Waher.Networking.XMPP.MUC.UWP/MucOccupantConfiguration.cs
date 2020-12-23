using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Contains information about a configuration to be made to an occupant in a room.
	/// </summary>
	public class MucOccupantConfiguration
	{
		private readonly string jid;
		private readonly string nickName;
		private readonly string reason;
		private readonly Affiliation? affiliation;
		private readonly Role? role;

		/// <summary>
		/// Contains information about a configuration to be made to an occupant in a room.
		/// </summary>
		/// <param name="Jid">JID of occupant in room.</param>
		/// <param name="Affiliation">Affiliation change, if specified.</param>
		public MucOccupantConfiguration(string Jid, Affiliation Affiliation)
			: this(Jid, string.Empty, Affiliation, null, string.Empty)
		{
		}

		/// <summary>
		/// Contains information about a configuration to be made to an occupant in a room.
		/// </summary>
		/// <param name="Jid">JID of occupant in room.</param>
		/// <param name="Affiliation">Affiliation change, if specified.</param>
		/// <param name="Reason">Optioanl Reason</param>
		public MucOccupantConfiguration(string Jid, Affiliation Affiliation, string Reason)
			: this(Jid, string.Empty, Affiliation, null, Reason)
		{
		}

		/// <summary>
		/// Contains information about a configuration to be made to an occupant in a room.
		/// </summary>
		/// <param name="Jid">JID of occupant in room.</param>
		/// <param name="NickName">Nick-name of occupant in room.</param>
		/// <param name="Affiliation">Affiliation change, if specified.</param>
		public MucOccupantConfiguration(string Jid, string NickName, Affiliation Affiliation)
			: this(Jid, NickName, Affiliation, null, string.Empty)
		{
		}

		/// <summary>
		/// Contains information about a configuration to be made to an occupant in a room.
		/// </summary>
		/// <param name="Jid">JID of occupant in room.</param>
		/// <param name="NickName">Nick-name of occupant in room.</param>
		/// <param name="Affiliation">Affiliation change, if specified.</param>
		/// <param name="Reason">Optioanl Reason</param>
		public MucOccupantConfiguration(string Jid, string NickName, Affiliation Affiliation, string Reason)
			: this(Jid, NickName, Affiliation, null, Reason)
		{
		}

		/// <summary>
		/// Contains information about a configuration to be made to an occupant in a room.
		/// </summary>
		/// <param name="NickName">Nick-name of occupant in room.</param>
		/// <param name="Role">Role change, if specified.</param>
		public MucOccupantConfiguration(string NickName, Role Role)
			: this(string.Empty, NickName, null, Role, string.Empty)
		{
		}

		/// <summary>
		/// Contains information about a configuration to be made to an occupant in a room.
		/// </summary>
		/// <param name="NickName">Nick-name of occupant in room.</param>
		/// <param name="Role">Role change, if specified.</param>
		/// <param name="Reason">Optioanl Reason</param>
		public MucOccupantConfiguration(string NickName, Role Role, string Reason)
			: this(string.Empty, NickName, null, Role, Reason)
		{
		}

		/// <summary>
		/// Contains information about a configuration to be made to an occupant in a room.
		/// </summary>
		/// <param name="Jid">JID of occupant in room.</param>
		/// <param name="NickName">Nick-Name of occupant in room.</param>
		/// <param name="Affiliation">Affiliation change, if specified.</param>
		/// <param name="Role">Role change, if specified.</param>
		public MucOccupantConfiguration(string Jid, string NickName, Affiliation? Affiliation, Role? Role)
			: this(Jid, NickName, Affiliation, Role, string.Empty)
		{
		}

		/// <summary>
		/// Contains information about a configuration to be made to an occupant in a room.
		/// </summary>
		/// <param name="Jid">JID of occupant in room.</param>
		/// <param name="NickName">Nick-Name of occupant in room.</param>
		/// <param name="Affiliation">Affiliation change, if specified.</param>
		/// <param name="Role">Role change, if specified.</param>
		/// <param name="Reason">Optioanl Reason</param>
		public MucOccupantConfiguration(string Jid, string NickName, Affiliation? Affiliation, Role? Role, string Reason)
		{
			this.jid = Jid;
			this.nickName = NickName;
			this.affiliation = Affiliation;
			this.role = Role;
			this.reason = Reason;
		}

		/// <summary>
		/// Bare JID  of occupant in room.
		/// </summary>
		public string Jid => this.jid;

		/// <summary>
		/// Nick-name of occupant in room.
		/// </summary>
		public string NickName => this.nickName;

		/// <summary>
		/// Affiliation change, if specified.
		/// </summary>
		public Affiliation? Affiliation => this.affiliation;

		/// <summary>
		/// Role change, if specified.
		/// </summary>
		public Role? Role => this.role;

		/// <summary>
		/// Optional reason.
		/// </summary>
		public string Reason => this.reason;
	}
}
