using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Sub-script text
	/// </summary>
	public class SubScript : MarkdownElementChildren
	{
		/// <summary>
		/// Sub-script text
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		public SubScript(MarkdownDocument Document, IEnumerable<MarkdownElement> ChildElements)
			: base(Document, ChildElements)
		{
		}

		/// <summary>
		/// Sub-script text
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Text">Subscript text.</param>
		public SubScript(MarkdownDocument Document, string Text)
			: base(Document, new MarkdownElement[] { new InlineText(Document, Text) })
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<sub>");

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.Append("</sub>");
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			Output.WriteStartElement("Run");
			Output.WriteAttributeString("Typography.Variants", "Subscript");

			foreach (MarkdownElement E in this.Children)
				E.GenerateXAML(Output, Settings, TextAlignment);

			Output.WriteEndElement();
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			StringBuilder sb = new StringBuilder();
			int i;

			base.GeneratePlainText(sb);

			foreach (char ch in sb.ToString())
			{
				i = nrmScriptLetters.IndexOf(ch);
				if (i < 0)
					Output.Append(ch);
				else
					Output.Append(subScriptLetters[i]);
			}
		}

		private const string nrmScriptLetters = "0123456789+-=()aeoxhklmnpst";
		private const string subScriptLetters = "₀₁₂₃₄₅₆₇₈₉₊₋₌₍₎ₐₑₒₓₕₖₗₘₙₚₛₜ";

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
			this.Export(Output, "SubScript");
		}

	}
}
