using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Delegate for node callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task NodeEventHandler(object Sender, NodeEventArgs e);

	/// <summary>
	/// Event arguments for node callback events.
	/// </summary>
    public class NodeEventArgs : IqResultEventArgs
    {
		private readonly string nodeName;

		/// <summary>
		/// Event arguments for node callback events.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="e">IQ result event arguments.</param>
		public NodeEventArgs(string NodeName, IqResultEventArgs e)
			: base(e)
		{
			this.nodeName = NodeName;
		}

		/// <summary>
		/// Event arguments for node callback events.
		/// </summary>
		/// <param name="e">IQ result event arguments.</param>
		public NodeEventArgs(NodeEventArgs e)
			: base(e)
		{
			this.nodeName = e.NodeName;
		}

		/// <summary>
		/// Node name.
		/// </summary>
		public string NodeName
		{
			get { return this.nodeName; }
		}
    }
}
