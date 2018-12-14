using System;
using System.Text;
using Waher.Content.Markdown;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Text
	/// </summary>
	public class Text : InlineElement
	{
		private string value;

		/// <summary>
		/// Text
		/// </summary>
		public string Value
		{
			get => this.value;
			set => this.value = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override bool IsWellDefined => !string.IsNullOrEmpty(this.value);

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<text>");
			Xml.Append(XML.Encode(this.value));
			Xml.Append("</text>");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		public override void GenerateMarkdown(StringBuilder Markdown, int SectionLevel, Contract Contract)
		{
			Markdown.Append(MarkdownDocument.Encode(this.value));
		}
	}
}
