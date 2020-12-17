using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Affiliation enumeration
	/// </summary>
	public enum Affiliation
	{
		/// <summary>
		/// Owner of room
		/// </summary>
		Owner = 0,

		/// <summary>
		/// Administrator of room
		/// </summary>
		Admin = 1,

		/// <summary>
		/// Member of room
		/// </summary>
		Member = 2,

		/// <summary>
		/// Outcast
		/// </summary>
		Outcast = 3,

		/// <summary>
		/// No affiliation
		/// </summary>
		None = 4
	}

	/// <summary>
	/// Role enumeration
	/// </summary>
	public enum Role
	{
		/// <summary>
		/// Room moderator
		/// </summary>
		Moderator = 0,

		/// <summary>
		/// Room participant
		/// </summary>
		Participant = 1,

		/// <summary>
		/// Room visitor
		/// </summary>
		Visitor = 2,

		/// <summary>
		/// No role
		/// </summary>
		None = 3
	}
}
