using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Footnote reference
	/// </summary>
	public class FootnoteReference : MarkdownElement
	{
		private string key;

		/// <summary>
		/// Footnote reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		public FootnoteReference(MarkdownDocument Document, string Key)
			: base(Document)
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
			string s;
			int Nr;

			if (this.Document.TryGetFootnoteNumber(this.key, out Nr))
			{
				s = Nr.ToString();

				Output.Append("<sup id=\"fnref-");
				Output.Append(s);
				Output.Append("\"><a href=\"#fn-");
				Output.Append(s);
				Output.Append("\" class=\"footnote-ref\">");
				Output.Append(s);
				Output.Append("</a></sup>");
			}
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			int Nr;

			if (this.Document.TryGetFootnoteNumber(this.key, out Nr))
			{
				Output.Append(" [");
				Output.Append(Nr.ToString());
				Output.Append("]");
			}
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.key;
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			string s;
			int Nr;

			if (this.Document.TryGetFootnoteNumber(this.key, out Nr))
			{
				Output.WriteStartElement("TextBlock");
				Output.WriteAttributeString("Text", Nr.ToString());

				Output.WriteStartElement("TextBlock.LayoutTransform");
				Output.WriteStartElement("TransformGroup");

				Output.WriteStartElement("ScaleTransform");
				Output.WriteAttributeString("ScaleX", s = CommonTypes.Encode(Settings.SuperscriptScale));
				Output.WriteAttributeString("ScaleY", s);
				Output.WriteEndElement();

				Output.WriteStartElement("TranslateTransform");
				Output.WriteAttributeString("Y", Settings.SuperscriptOffset.ToString());
				Output.WriteEndElement();

				Output.WriteEndElement();
				Output.WriteEndElement();
				Output.WriteEndElement();
			}
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return true; }
		}
	}
}
