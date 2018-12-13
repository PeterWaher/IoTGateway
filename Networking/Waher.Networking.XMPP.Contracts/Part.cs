using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// How the parts of the contract are defined.
	/// </summary>
	public enum ContractParts
	{
		/// <summary>
		/// Contract is open. Anyone can sign.
		/// </summary>
		Open,

		/// <summary>
		/// Contract is only a template. No-one can sign.
		/// </summary>
		TemplateOnly,

		/// <summary>
		/// Only explicitly defined parts can sign.
		/// </summary>
		ExplicitlyDefined
	}

	/// <summary>
	/// Class defining a part in a contract
	/// </summary>
	public class Part
	{
		private string legalId = string.Empty;
		private string role = string.Empty;

		/// <summary>
		/// Legal identity of part
		/// </summary>
		public string LegalId
		{
			get => this.legalId;
			set => this.legalId = value;
		}

		/// <summary>
		/// Role of the part in the contract
		/// </summary>
		public string Role
		{
			get => this.role;
			set => this.role = value;
		}

	}
}
