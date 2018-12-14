using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

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
			Serialize(Xml, this.Elements, "item");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		public override void GenerateMarkdown(StringBuilder Markdown, int SectionLevel, Contract Contract)
		{
			foreach (InlineElement E in this.Elements)
				E.GenerateMarkdown(Markdown, SectionLevel, Contract);

			Markdown.AppendLine();
		}
	}
}
