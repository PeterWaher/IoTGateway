using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Superscript text
	/// </summary>
	public class Superscript : Formatting
	{
		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Serialize(Xml, this.Elements, "super");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		public override void GenerateMarkdown(StringBuilder Markdown, int SectionLevel, Contract Contract)
		{
			Markdown.Append("^[");

			foreach (InlineElement E in this.Elements)
				E.GenerateMarkdown(Markdown, SectionLevel, Contract);

			Markdown.Append(']');
		}
	}
}
