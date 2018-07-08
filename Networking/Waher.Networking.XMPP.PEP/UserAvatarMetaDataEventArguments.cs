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
		private PepClient pepClient;

		internal UserAvatarMetaDataEventArguments(UserAvatarMetaData AvatarMetaData, PersonalEventNotificationEventArgs e):
			base(e)
		{
			this.avatarMetaData = AvatarMetaData;
			this.pepClient = PepClient;
		}

		/// <summary>
		/// User avatar metadata.
		/// </summary>
		public UserAvatarMetaData AvatarMetaData => this.avatarMetaData;

		/// <summary>
		/// Personal Eventing Protocol (PEP) Client.
		/// </summary>
		public PepClient PepClient => this.pepClient;

		/// <summary>
		/// Gets an avatar published by a user using the Personal Eventing Protocol
		/// </summary>
		/// <param name="Reference">Avatar reference, selected from	an <see cref="UserAvatarMetaData"/> event.</param>
		/// <param name="Callback">Method to call when avatar has been retrieved.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetUserAvatarData(UserAvatarReference Reference, UserAvatarImageEventHandler Callback, object State)
		{
			this.pepClient.GetUserAvatarData(this.FromBareJID, Reference, Callback, State);
		}

	}
}
