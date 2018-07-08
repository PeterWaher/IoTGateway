using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// Delegate for user avatar metadata events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="AvatarMetaData"></param>
	public delegate void UserAvatarMetaDataEventHandler(object Sender, UserAvatarMetaDataEventArguments AvatarMetaData);

	/// <summary>
	/// Event arguments for user avatar metadata events.
	/// </summary>
	public class UserAvatarMetaDataEventArguments : PersonalEventNotificationEventArgs
	{
		private UserAvatarMetaData avatarMetaData;

		internal UserAvatarMetaDataEventArguments(UserAvatarMetaData AvatarMetaData, PersonalEventNotificationEventArgs e):
			base(e)
		{
			this.avatarMetaData = AvatarMetaData;
		}

		/// <summary>
		/// User avatar metadata.
		/// </summary>
		public UserAvatarMetaData AvatarMetaData => this.avatarMetaData;
    }
}
