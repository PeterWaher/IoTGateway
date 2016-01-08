using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents an unnumbered item in an ordered list.
	/// </summary>
	public class UnnumberedItem : MarkdownElementSingleChild
	{
		private string prefix;

		/// <summary>
		/// Represents an unnumbered item in an ordered list.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Prefix">Prefix, in plain text mode.</param>
		/// <param name="Child">Child element.</param>
		public UnnumberedItem(MarkdownDocument Document, string Prefix, MarkdownElement Child)
			: base(Document, Child)
		{
			this.prefix = Prefix;
		}

		/// <summary>
		/// Prefix, in plain text mode.
		/// </summary>
		public string Prefix
		{
			get { return this.prefix; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<li>");
			this.Child.GenerateHTML(Output);
			Output.AppendLine("</li>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.prefix);
			this.Child.GeneratePlainText(Output);
		}
	}
}
