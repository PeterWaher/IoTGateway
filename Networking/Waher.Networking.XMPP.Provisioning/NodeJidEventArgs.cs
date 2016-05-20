using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event argument base class for node information and JID events.
	/// </summary>
	public abstract class NodeJidEventArgs : NodeEventArgs
	{
		private string jid;

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
