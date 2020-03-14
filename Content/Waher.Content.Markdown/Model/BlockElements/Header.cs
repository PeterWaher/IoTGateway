using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a header in a markdown document.
	/// </summary>
	public class Header : BlockElementChildren
	{
		private readonly string row;
		private readonly string id;
		private readonly int level;
		private readonly bool prefix;

		/// <summary>
		/// Represents a header in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Level">Header level.</param>
		/// <param name="Prefix">If header was defined with a prefix (true) or with an underline (false).</param>
		/// <param name="Row">Header definition.</param>
		/// <param name="Children">Child elements.</param>
		public Header(MarkdownDocument Document, int Level, bool Prefix, string Row, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
			this.level = Level;
			this.prefix = Prefix;
			this.row = Row;

			StringBuilder sb = new StringBuilder();

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(sb);

			string s = sb.ToString();
			sb.Clear();

			bool FirstCharInWord = false;

			foreach (char ch in s)
			{
				if (!char.IsLetter(ch) && !char.IsDigit(ch))
				{
					FirstCharInWord = true;
					continue;
				}

				if (FirstCharInWord)
				{
					sb.Append(char.ToUpper(ch));
					FirstCharInWord = false;
				}
				else
					sb.Append(char.ToLower(ch));
			}

			this.id = sb.ToString();
		}

		/// <summary>
		/// Header level.
		/// </summary>
		public int Level
		{
			get { return this.level; }
		}

		/// <summary>
		/// ID of header.
		/// </summary>
		public string Id
		{
			get { return this.id; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			if (this.prefix)
			{
				Output.Append(this.row);
				Output.Append(' ');
			}

			base.GenerateMarkdown(Output);
			Output.AppendLine();

			if (!this.prefix)
				Output.AppendLine(this.row);

			Output.AppendLine();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<h");
			Output.Append(this.level.ToString());

			if (!string.IsNullOrEmpty(this.id))
			{
				Output.Append(" id=\"");
				Output.Append(XML.HtmlAttributeEncode(this.id));
				Output.Append("\"");
			}

			if (this.Document.IncludesTableOfContents)
				Output.Append(" class=\"tocReference\"");

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

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			Output.WriteStartElement("TextBlock");
			Output.WriteAttributeString("TextWrapping", "Wrap");
			Output.WriteAttributeString("Margin", Settings.ParagraphMargins);
			if (TextAlignment != TextAlignment.Left)
				Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

			if (this.level > 0 && this.level <= Settings.HeaderFontSize.Length)
			{
				Output.WriteAttributeString("FontSize", Settings.HeaderFontSize[this.level - 1].ToString());
				Output.WriteAttributeString("Foreground", Settings.HeaderForegroundColor[this.level - 1].ToString());
			}

			foreach (MarkdownElement E in this.Children)
				E.GenerateXAML(Output, Settings, TextAlignment);

			Output.WriteEndElement();
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return false; }
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("Header");
			Output.WriteAttributeString("id", this.id);
			Output.WriteAttributeString("level", this.level.ToString());
			this.ExportChildren(Output);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(IEnumerable<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new Header(Document, this.level, this.prefix, this.row, Children);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is Header x &&
				x.level == this.level &&
				x.prefix == this.prefix &&
				x.row == this.row &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Header x &&
				this.level == x.level &&
				this.prefix == x.prefix &&
				this.row == x.row &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.level.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.prefix.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.row?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
