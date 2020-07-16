using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Markdown.Model.Atoms;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Inline source code.
	/// </summary>
	public class InlineCode : MarkdownElement, IEditableText
	{
		private readonly string code;

		/// <summary>
		/// Inline source code.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Code">Inline Code.</param>
		public InlineCode(MarkdownDocument Document, string Code)
			: base(Document)
		{
			if (Code.StartsWith(" "))
				Code = Code.Substring(1);

			if (Code.EndsWith(" "))
				Code = Code.Substring(0, Code.Length - 1);

			this.code = Code;
		}

		/// <summary>
		/// Inline code.
		/// </summary>
		public string Code
		{
			get { return this.code; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			Output.Append('`');
			Output.Append(this.code);
			Output.Append('`');
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<code>");
			Output.Append(XML.HtmlValueEncode(this.code));
			Output.Append("</code>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.code);
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			Output.WriteStartElement("TextBlock");
			Output.WriteAttributeString("xml", "space", null, "preserve");
			Output.WriteAttributeString("TextWrapping", "Wrap");
			Output.WriteAttributeString("FontFamily", "Courier New");
			if (TextAlignment != TextAlignment.Left)
				Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

			Output.WriteValue(this.code);

			Output.WriteEndElement();
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXamarinForms(XmlWriter Output, TextAlignment TextAlignment)
		{
			InlineText.GenerateInlineFormattedTextXamarinForms(Output, this);
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return true; }
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("InlineCode");
			Output.WriteCData(this.code);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is InlineCode x &&
				this.code == x.code &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.code?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Return an enumeration of the editable code as atoms.
		/// </summary>
		/// <returns>Atoms.</returns>
		public IEnumerable<Atom> Atomize()
		{
			LinkedList<Atom> Result = new LinkedList<Atom>();

			foreach (char ch in this.code)
				Result.AddLast(new InlineCodeCharacter(this.Document, this, ch));

			return Result;
		}

		/// <summary>
		/// Assembles a markdown element from a sequence of atoms.
		/// </summary>
		/// <param name="Document">Document that will contain the new element.</param>
		/// <param name="Text">Assembled text.</param>
		/// <returns>Assembled markdown element.</returns>
		public MarkdownElement Assemble(MarkdownDocument Document, string Text)
		{
			return new InlineCode(Document, Text);
		}

	}
}
