using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event arguments for IsFriend callbacks.
	/// </summary>
	public class IsFriendEventArgs : JidEventArgs
	{
		private bool friend;

		internal IsFriendEventArgs(IqResultEventArgs e, object State, string JID, bool Friend)
			: base(e, State, JID)
		{
			this.friend = Friend;
		}

		/// <summary>
		/// If the two are friends.
		/// </summary>
		public bool Friend
		{
			get { return this.friend; }
		}
	}
}
