using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.P2P.E2E;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for contract signature event handlers.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate void ContractSignedEventHandler(object Sender, ContractSignedEventArgs e);

	/// <summary>
	/// Event arguments for contract signature events
	/// </summary>
	public class ContractSignedEventArgs : ContractReferenceEventArgs
	{
		private readonly string legalId;

		/// <summary>
		/// Event arguments for contract signature events
		/// </summary>
		public ContractSignedEventArgs(string ContractId, string LegalId)
			: base(ContractId)
		{
			this.legalId = LegalId;
		}

		/// <summary>
		/// ID of legal identity signing the contract.
		/// </summary>
		public string LegalId => this.legalId;
	}
}
