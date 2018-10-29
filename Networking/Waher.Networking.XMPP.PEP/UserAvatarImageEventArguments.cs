using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.PubSub;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// Delegate for user avatar metadata events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="AvatarImage"></param>
	public delegate void UserAvatarImageEventHandler(object Sender, UserAvatarImageEventArguments AvatarImage);

	/// <summary>
	/// Event arguments for user avatar metadata events.
	/// </summary>
	public class UserAvatarImageEventArguments : ItemsEventArgs
	{
		private readonly UserAvatarImage avatarImage;

		internal UserAvatarImageEventArguments(UserAvatarImage AvatarImage, ItemsEventArgs e)
			: base(e)
		{
			this.avatarImage = AvatarImage;
		}

		/// <summary>
		/// User avatar image.
		/// </summary>
		public UserAvatarImage AvatarImage => this.avatarImage;
    }
}
