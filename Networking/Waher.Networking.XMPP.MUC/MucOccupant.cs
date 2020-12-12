using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Multi-User Chat room occupant.
	/// </summary>
	public class MucOccupant
	{
		private readonly MultiUserChatClient client;
		private readonly string roomId;
		private readonly string domain;
		private string nickName;
		private string jid;
		private Affiliation affiliation;
		private Role role;

		/// <summary>
		/// Multi-User Chat room occupant.
		/// </summary>
		/// <param name="Client">MUC Client</param>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the MUC room.</param>
		/// <param name="NickName">Nick-name</param>
		/// <param name="Jid">JID</param>
		/// <param name="Affiliation">Affiliation</param>
		/// <param name="Role">Role</param>
		public MucOccupant(MultiUserChatClient Client, string RoomId, string Domain, 
			string NickName, string Jid, Affiliation Affiliation, Role Role)
		{
			this.client = Client;
			this.roomId = RoomId;
			this.domain = Domain;
			this.nickName = NickName;
			this.jid = Jid;
			this.affiliation = Affiliation;
			this.role = Role;
		}

		/// <summary>
		/// MUC Client
		/// </summary>
		public MultiUserChatClient Client => this.client;

		/// <summary>
		/// Room ID
		/// </summary>
		public string RoomId => this.roomId;

		/// <summary>
		/// Domain hosting the MUC room.
		/// </summary>
		public string Domain => this.domain;

		/// <summary>
		/// Affiliation assigned to occupant.
		/// </summary>
		public Affiliation Affiliation
		{
			get => this.affiliation;
			internal set => this.affiliation = value;
		}

		/// <summary>
		/// Role assigned to occupant.
		/// </summary>
		public Role Role
		{
			get => this.role;
			internal set => this.role = value;
		}

		/// <summary>
		/// Nick-name in room.
		/// </summary>
		public string NickName
		{
			get => this.nickName;
			set => this.nickName = value;
		}

		/// <summary>
		/// JID of occupant
		/// </summary>
		public string Jid
		{
			get => this.jid;
			set => this.jid = value;
		}
	}
}
