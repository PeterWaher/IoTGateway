using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.PubSub.Events
{
	/// <summary>
	/// Event arguments for affiliation list callback events.
	/// </summary>
	public class AffiliationsEventArgs : NodeEventArgs
    {
		private readonly Affiliation[] affiliations;

		/// <summary>
		/// Event arguments for affiliation list callback events.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Affiliations">Available affiliations.</param>
		/// <param name="e">IQ result event arguments.</param>
		public AffiliationsEventArgs(string NodeName, Affiliation[] Affiliations, IqResultEventArgs e)
			: base(NodeName, e)
		{
			this.affiliations = Affiliations;
		}

		/// <summary>
		/// Available affiliations.
		/// </summary>
		public Affiliation[] Affiliations => this.affiliations;
    }
}
