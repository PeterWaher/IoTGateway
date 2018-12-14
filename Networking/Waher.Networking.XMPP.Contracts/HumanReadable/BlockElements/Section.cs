using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// A section consisting of a header and a body.
	/// </summary>
	public class Section : Blocks
	{
		private InlineElement[] header;

		/// <summary>
		/// Header elements
		/// </summary>
		public InlineElement[] Header
		{
			get => this.header;
			set => this.header = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override bool IsWellDefined
		{
			get
			{
				foreach (InlineElement E in this.header)
				{
					if (E == null || !E.IsWellDefined)
						return false;
				}

				return base.IsWellDefined;
			}
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<section>");
			Serialize(Xml, this.Header, "header");
			Serialize(Xml, this.Body, "body");
			Xml.Append("</section>");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		public override void GenerateMarkdown(StringBuilder Markdown, int SectionLevel, Contract Contract)
		{
			StringBuilder Markdown2;

			if (SectionLevel >= 3)
			{
				Markdown2 = Markdown;
				Markdown.Append(new string('#', SectionLevel));
				Markdown.Append(' ');
			}
			else
				Markdown2 = new StringBuilder();

			foreach (InlineElement E in this.header)
				E.GenerateMarkdown(Markdown2, SectionLevel, Contract);

			if (SectionLevel < 3)
			{
				string s = Markdown2.ToString();
				Markdown.AppendLine(s);
				Markdown.Append(new string(SectionLevel == 1 ? '=' : '-', s.Length + 3));
			}

			Markdown.AppendLine();
			Markdown.AppendLine();

			base.GenerateMarkdown(Markdown, SectionLevel + 1, Contract);
		}

	}
}
