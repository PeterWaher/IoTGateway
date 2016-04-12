using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Base class for CanRead and CanControl callback event arguments.
	/// </summary>
	public abstract class NodeEventArgs : JidEventArgs
	{
		private ThingReference[] nodes;

		internal NodeEventArgs(IqResultEventArgs e, object State, string JID, ThingReference[] Nodes)
			: base(e, State, JID)
		{
			this.nodes = Nodes;
		}

		/// <summary>
		/// Nodes allowed to read, as long as <see cref="CanRead"/> is true. If null, no node restrictions exist.
		/// </summary>
		public ThingReference[] Nodes
		{
			get { return this.nodes; }
		}
	}
}
