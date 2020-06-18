using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Avatar
{
	/// <summary>
	/// Delegate for avatar events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task AvatarEventHandler(object Sender, AvatarEventArgs e);

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
