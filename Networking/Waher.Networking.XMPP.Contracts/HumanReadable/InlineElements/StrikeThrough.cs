using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Text that is stricken through
	/// </summary>
	public class StrikeThrough : Formatting
	{
		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Serialize(Xml, this.Elements, "strikeThrough");
		}
	}
}
