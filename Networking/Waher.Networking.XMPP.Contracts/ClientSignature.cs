using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Represents a digital signature on a contract.
	/// </summary>
	public class ClientSignature : Signature
	{
		private string legalId = null;
		private string bareJid = null;
		private string role = null;
		private bool transferable = false;

		/// <summary>
		/// ID of legal identity signing the contract.
		/// </summary>
		public string LegalId
		{
			get => this.legalId;
			set => this.legalId = value;
		}

		/// <summary>
		/// ID of legal identity signing the contract, as an URI.
		/// </summary>
		public Uri LegalIdUri => ContractsClient.LegalIdUri(this.legalId);

		/// <summary>
		/// ID of legal identity signing the contract, as an URI string.
		/// </summary>
		public string LegalIdUriString => ContractsClient.LegalIdUriString(this.legalId);

		/// <summary>
		/// Bare JID of the client used to generate the signature.
		/// </summary>
		public string BareJid
		{
			get => this.bareJid;
			set => this.bareJid = value;
		}

		/// <summary>
		/// Role of the legal identity in the contract.
		/// </summary>
		public string Role
		{
			get => this.role;
			set => this.role = value;
		}

		/// <summary>
		/// If the signature is transferable to contracts based on the current contract as a template,
		/// and if no parameters and attributes change in the contract.
		/// </summary>
		public bool Transferable
		{
			get => this.transferable;
			set => this.transferable = value;
		}

		/// <summary>
		/// Serializes the signature, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<signature bareJid=\"");
			Xml.Append(XML.Encode(this.bareJid));
			Xml.Append("\" legalId=\"");
			Xml.Append(XML.Encode(this.legalId));
			Xml.Append("\" role=\"");
			Xml.Append(XML.Encode(this.role));
			Xml.Append("\" timestamp=\"");
			Xml.Append(XML.Encode(this.Timestamp));
			Xml.Append("\">");
            Xml.Append(Convert.ToBase64String(this.DigitalSignature));
            Xml.Append("</signature>");
        }
    }
}
