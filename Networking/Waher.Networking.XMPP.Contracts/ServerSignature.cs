using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Represents a server signature on a contract.
	/// </summary>
	public class ServerSignature : Signature
	{
		/// <summary>
		/// Serializes the signature, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<serverSignature");
            Xml.Append(" timestamp=\"");
			Xml.Append(XML.Encode(this.Timestamp));
			Xml.Append("\">");
            Xml.Append(Convert.ToBase64String(this.DigitalSignature));
			Xml.Append("</serverSignature>");
        }
    }
}
