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

			if (this.S1 != null)
			{
				Xml.Append(" s1=\"");
				Xml.Append(Convert.ToBase64String(this.S1));
				Xml.Append('"');
			}

			if (this.S2 != null)
			{
				Xml.Append(" s2=\"");
				Xml.Append(Convert.ToBase64String(this.S2));
				Xml.Append('"');
			}

			Xml.Append(" timestamp=\"");
			Xml.Append(XML.Encode(this.Timestamp));
			Xml.Append("\"/>");
		}
	}
}
