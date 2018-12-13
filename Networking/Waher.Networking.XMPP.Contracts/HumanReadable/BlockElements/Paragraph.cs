using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Paragaph of text.
	/// </summary>
	public class Paragraph : InlineBlock
	{
		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Serialize(Xml, this.Elements, "paragraph");
		}
	}
}
