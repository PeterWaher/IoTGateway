using System;
using Waher.Things;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event argument base class for node information and JID events.
	/// </summary>
	public abstract class NodeJidEventArgs : NodeEventArgs
	{
		private readonly string jid;

		internal NodeJidEventArgs(IqEventArgs e, ThingReference Node, string Jid)
			: base(e, Node)
		{
			this.jid = Jid;
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
