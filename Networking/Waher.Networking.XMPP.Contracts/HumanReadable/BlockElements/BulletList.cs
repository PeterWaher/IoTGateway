using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Bullet list
	/// </summary>
	public class BulletList : ItemList 
	{
		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Serialize(Xml, this.Items, "bulletItems");
		}
	}
}
