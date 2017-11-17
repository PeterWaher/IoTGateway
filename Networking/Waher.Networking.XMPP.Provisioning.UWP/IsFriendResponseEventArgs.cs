using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Delegate for IsFriend callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments.</param>
	public delegate void IsFriendCallback(object Sender, IsFriendResponseEventArgs e);

	/// <summary>
	/// Event arguments for IsFriend callbacks.
	/// </summary>
	public class IsFriendResponseEventArgs : JidEventArgs
	{
		private bool friend;

		internal IsFriendResponseEventArgs(IqResultEventArgs e, object State, string JID, bool Friend)
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
