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
		Owner,

		/// <summary>
		/// Administrator of room
		/// </summary>
		Admin,

		/// <summary>
		/// Member of room
		/// </summary>
		Member,

		/// <summary>
		/// Outcast
		/// </summary>
		Outcast,

		/// <summary>
		/// No affiliation
		/// </summary>
		None
	}

	/// <summary>
	/// Role enumeration
	/// </summary>
	public enum Role
	{
		/// <summary>
		/// Room moderator
		/// </summary>
		Moderator,

		/// <summary>
		/// Room participant
		/// </summary>
		Participant,

		/// <summary>
		/// Room visitor
		/// </summary>
		Visitor,

		/// <summary>
		/// No role
		/// </summary>
		None
	}
}
