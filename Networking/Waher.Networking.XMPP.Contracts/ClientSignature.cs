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

		/// <summary>
		/// ID of legal identity signing the contract.
		/// </summary>
		public string LegalId
		{
			get => this.legalId;
			set => this.legalId = value;
		}

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

			if (this.S1 != null)
			{
				Xml.Append("\" s1=\"");
				Xml.Append(Convert.ToBase64String(this.S1));
			}

			if (this.S2 != null)
			{
				Xml.Append("\" s2=\"");
				Xml.Append(Convert.ToBase64String(this.S2));
			}

			Xml.Append("\" timestamp=\"");
			Xml.Append(XML.Encode(this.Timestamp));
			Xml.Append("\"/>");
		}
	}
}
