using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Footnote
	/// </summary>
	public class Footnote : MarkdownElementChildren
	{
		private string key;

		/// <summary>
		/// Footnote reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="ChildElements">Child elements.</param>
		public Footnote(MarkdownDocument Document, string Key, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
			this.key = Key;
		}

		/// <summary>
		/// Footnote reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="ChildElements">Child elements.</param>
		public Footnote(MarkdownDocument Document, string Key, params MarkdownElement[] Children)
			: base(Document, Children)
		{
			this.key = Key;
		}

		/// <summary>
		/// Footnote key
		/// </summary>
		public string Key
		{
			get { return this.key; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				E.GeneratePlainText(Output);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.key;
		}
	}
}
