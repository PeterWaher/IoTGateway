using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a header in a markdown document.
	/// </summary>
	public class Header : MarkdownElementChildren
	{
		private int level;

		/// <summary>
		/// Represents a header in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Level">Header level.</param>
		/// <param name="Children">Child elements.</param>
		public Header(MarkdownDocument Document, int Level, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
			this.level = Level;
		}

		/// <summary>
		/// Header level.
		/// </summary>
		public int Level
		{
			get { return this.level; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<h");
			Output.Append(this.level.ToString());
			Output.Append('>');

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.Append("</h");
			Output.Append(this.level.ToString());
			Output.AppendLine(">");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			if (this.level <= 2)
			{
				int Len = Output.Length;

				foreach (MarkdownElement E in this.Children)
					E.GeneratePlainText(Output);

				Len = Output.Length - Len + 3;
				Output.AppendLine();
				Output.AppendLine(new string(this.level == 1 ? '=' : '-', Len));
				Output.AppendLine();
			}
			else
			{
				Output.Append(new string('#', this.level));
				Output.Append(' ');

				foreach (MarkdownElement E in this.Children)
					E.GeneratePlainText(Output);

				Output.AppendLine();
				Output.AppendLine();
			}
		}

	}
}
