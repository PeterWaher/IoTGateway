using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Markdown.Model.Atoms;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Unformatted text.
	/// </summary>
	public class InlineText : MarkdownElement, IEditableText
	{
		private string value;

		/// <summary>
		/// Unformatted text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Value">Inline text.</param>
		public InlineText(MarkdownDocument Document, string Value)
			: base(Document)
		{
			this.value = Value;
		}

		/// <summary>
		/// Unformatted text.
		/// </summary>
		public string Value
		{
			get { return this.value; }
			internal set { this.value = value; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			Output.Append(MarkdownDocument.Encode(this.value));
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append(XML.HtmlValueEncode(this.value));
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.value);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.value;
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			Output.WriteValue(this.value);
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
			Output.WriteElementString("InlineText", this.value);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is InlineText x &&
				this.value == x.value &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.value?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Return an enumeration of the editable text as atoms.
		/// </summary>
		/// <returns>Atoms.</returns>
		public IEnumerable<Atom> Atomize()
		{
			foreach (char ch in this.value)
				yield return new InlineTextCharacter(this.Document, this, ch);
		}

		/// <summary>
		/// Assembles a markdown element from a sequence of atoms.
		/// </summary>
		/// <param name="Document">Document that will contain the new element.</param>
		/// <param name="Text">Assembled text.</param>
		/// <returns>Assembled markdown element.</returns>
		public MarkdownElement Assemble(MarkdownDocument Document, string Text)
		{
			return new InlineText(Document, Text);
		}
	}
}
