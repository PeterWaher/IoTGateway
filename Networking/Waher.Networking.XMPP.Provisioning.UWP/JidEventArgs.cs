using System;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event arguments for JID callbacks.
	/// </summary>
	public class JidEventArgs : IqResultEventArgs
	{
		private readonly string jid;

		internal JidEventArgs(IqResultEventArgs e, object State, string JID)
			: base(e)
		{
			this.State = State;
			this.jid = JID;
		}

		/// <summary>
		/// JID.
		/// </summary>
		public string JID
		{
			get { return this.jid; }
		}
	}
}
