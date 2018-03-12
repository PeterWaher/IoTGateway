using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Delegate for affiliation list callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void AffiliationListEventHandler(object Sender, AffiliationListEventArgs e);

	/// <summary>
	/// Event arguments for affiliation list callback events.
	/// </summary>
	public class AffiliationListEventArgs : NodeEventArgs
    {
		private Dictionary<string, Affiliation> affiliations;

		/// <summary>
		/// Event arguments for affiliation list callback events.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Affiliations">Available affiliations.</param>
		/// <param name="e">IQ result event arguments.</param>
		public AffiliationListEventArgs(string NodeName, 
			Dictionary<string, Affiliation> Affiliations, IqResultEventArgs e)
			: base(NodeName, e)
		{
			this.affiliations = Affiliations;
		}

		/// <summary>
		/// Available affiliations.
		/// </summary>
		public Dictionary<string, Affiliation> Affiliations
		{
			get { return this.affiliations; }
		}
    }
}
