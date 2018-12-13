using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// An item in an item list.
	/// </summary>
	public class Item : InlineBlock
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
