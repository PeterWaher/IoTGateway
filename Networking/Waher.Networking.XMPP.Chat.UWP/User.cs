using System;
using System.Collections.Generic;
using System.Text;
using Waher.Security;

namespace Waher.Networking.XMPP.Chat
{
	/// <summary>
	/// Chat user
	/// </summary>
	public class User : IUser
	{
		private readonly Dictionary<string, bool> privileges = new Dictionary<string, bool>();
		private readonly string fullJid;

		/// <summary>
		/// Chat user
		/// </summary>
		/// <param name="FullJID">Full JID</param>
		public User(string FullJID)
		{
			this.fullJid = FullJID;
		}

		/// <summary>
		/// User Name.
		/// </summary>
		public string UserName => this.fullJid;

		/// <summary>
		/// Password Hash
		/// </summary>
		public string PasswordHash => string.Empty;

		/// <summary>
		/// Type of password hash. The empty stream means a clear-text password.
		/// </summary>
		public string PasswordHashType => string.Empty;

		/// <summary>
		/// If the user has a given privilege.
		/// </summary>
		/// <param name="Privilege">Privilege.</param>
		/// <returns>If the user has the corresponding privilege.</returns>
		public bool HasPrivilege(string Privilege)
		{
			lock (this.privileges)
			{
				while (!string.IsNullOrEmpty(Privilege))
				{
					if (this.privileges.TryGetValue(Privilege, out bool b))
						return b;

					int i = Privilege.LastIndexOf('.');
					if (i < 0)
						break;
					else
						Privilege = Privilege.Substring(0, i);
				}

				return false;
			}
		}

		internal void SetPrivilege(string Privilege, bool Value)
		{
			lock (this.privileges)
			{
				this.privileges[Privilege] = Value;
			}
		}

	}
}
