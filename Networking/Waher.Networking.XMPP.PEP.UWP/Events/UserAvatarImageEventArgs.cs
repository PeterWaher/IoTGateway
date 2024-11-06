using Waher.Networking.XMPP.PubSub.Events;

namespace Waher.Networking.XMPP.PEP.Events
{
	/// <summary>
	/// Event arguments for user avatar metadata events.
	/// </summary>
	public class UserAvatarImageEventArgs : ItemsEventArgs
	{
		private readonly UserAvatarImage avatarImage;

		internal UserAvatarImageEventArgs(UserAvatarImage AvatarImage, ItemsEventArgs e)
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
