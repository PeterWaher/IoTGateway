using Waher.Networking.XMPP.Events;
using Waher.Things;

namespace Waher.Networking.XMPP.Provisioning.Events
{
	/// <summary>
	/// Event arguments for node callback methods.
	/// </summary>
	public class NodeResultEventArgs : JidEventArgs
	{
		private readonly ThingReference node;

		internal NodeResultEventArgs(IqResultEventArgs e, object State, string JID, ThingReference Node)
			: base(e, State, JID)
		{
			this.node = Node;
		}

		/// <summary>
		/// Node reference.
		/// </summary>
		public ThingReference Node => this.node;
	}
}
