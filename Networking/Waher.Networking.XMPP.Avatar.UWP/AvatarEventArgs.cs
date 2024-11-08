using System;

namespace Waher.Networking.XMPP.Avatar
{
	/// <summary>
	/// Event arguments for avatar events.
	/// </summary>
	public class AvatarEventArgs : EventArgs
	{
		private readonly string bareJid;
		private readonly Avatar avatar;

		/// <summary>
		/// Event arguments for avatar events.
		/// </summary>
		/// <param name="BareJid">Bare JID of avatar.</param>
		/// <param name="Avatar">Avatar</param>
		public AvatarEventArgs(string BareJid, Avatar Avatar)
		{
			this.bareJid = BareJid;
			this.avatar = Avatar;
		}

		/// <summary>
		/// Bare JID of avatar.
		/// </summary>
		public string BareJid => this.bareJid;

		/// <summary>
		/// Avatar, or null if removed.
		/// </summary>
		public Avatar Avatar => this.avatar;
	}
}
