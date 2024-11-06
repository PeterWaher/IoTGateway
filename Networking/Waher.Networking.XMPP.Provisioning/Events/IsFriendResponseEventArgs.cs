using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Provisioning.Events
{
	/// <summary>
	/// Event arguments for IsFriend callbacks.
	/// </summary>
	public class IsFriendResponseEventArgs : JidEventArgs
	{
		private readonly bool friend;

		internal IsFriendResponseEventArgs(IqResultEventArgs e, object State, string JID, bool Friend)
			: base(e, State, JID)
		{
			this.friend = Friend;
		}

		/// <summary>
		/// If the two are friends.
		/// </summary>
		public bool Friend => this.friend;
	}
}
