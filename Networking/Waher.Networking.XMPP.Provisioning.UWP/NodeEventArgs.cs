using System;
using Waher.Things;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event argument base class for node information events.
	/// </summary>
	public class NodeEventArgs : IqEventArgs
	{
		private ThingReference node;

		internal NodeEventArgs(IqEventArgs e, ThingReference Node)
			: base(e)
		{
			this.node = Node;
		}

		/// <summary>
		/// Node reference.
		/// </summary>
		public ThingReference Node
		{
			get { return this.node; }
			set { this.node = value; }
		}
	}
}
