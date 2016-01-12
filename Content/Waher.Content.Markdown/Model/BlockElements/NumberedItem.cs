using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a numbered item in an ordered list.
	/// </summary>
	public class NumberedItem : MarkdownElementSingleChild
	{
		private int number;

		/// <summary>
		/// Represents a numbered item in an ordered list.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Number">Number associated with item.</param>
		/// <param name="Child">Child element.</param>
		public NumberedItem(MarkdownDocument Document, int Number, MarkdownElement Child)
			: base(Document, Child)
		{
			this.number = Number;
		}

		/// <summary>
		/// Number associated with item.
		/// </summary>
		public int Number
		{
			get { return this.number; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<li value=\"");
			Output.Append(this.number.ToString());
			Output.Append("\">");
			
			this.Child.GenerateHTML(Output);

			Output.AppendLine("</li>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.number.ToString());
			Output.Append(". ");

			StringBuilder sb = new StringBuilder();
			this.Child.GeneratePlainText(sb);

			string s = sb.ToString();

			Output.Append(s);

			if (!s.EndsWith("\n"))
				Output.AppendLine();
		}
	}
}
