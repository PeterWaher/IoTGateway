using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a definition list in a markdown document.
	/// </summary>
	public class DefinitionList : BlockElementChildren
	{
		/// <summary>
		/// Represents a definition list in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public DefinitionList(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Represents a definition list in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public DefinitionList(MarkdownDocument Document, params MarkdownElement[] Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override async Task GenerateMarkdown(StringBuilder Output)
		{
			await base.GenerateMarkdown(Output);
			Output.AppendLine();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			Output.AppendLine("<dl>");

			foreach (MarkdownElement E in this.Children)
				await E.GenerateHTML(Output);

			Output.AppendLine("</dl>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override async Task GeneratePlainText(StringBuilder Output)
		{
			StringBuilder sb = new StringBuilder();
			string s;
			string s2 = Environment.NewLine + Environment.NewLine;
			bool LastIsParagraph = false;
			bool FirstTerm = true;

			s = Output.ToString();
			if (!s.EndsWith(Environment.NewLine) && !string.IsNullOrEmpty(s))
				Output.AppendLine();

			foreach (MarkdownElement E in this.Children)
			{
				if (E is DefinitionTerms)
				{
					if (FirstTerm)
						FirstTerm = false;
					else
						Output.AppendLine();
				}

				await E.GeneratePlainText(sb);
				s = sb.ToString();
				sb.Clear();
				Output.Append(s);

				LastIsParagraph = s.EndsWith(s2);
			}

			if (!LastIsParagraph)
				Output.AppendLine();
		}

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		internal override bool OutsideParagraph => true;

		/// <summary>
		/// If elements of this type should be joined over paragraphs.
		/// </summary>
		internal override bool JoinOverParagraphs => true;

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GenerateXAML(Output, TextAlignment);
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GenerateXamarinForms(Output, State);
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement => false;

		/// <summary>
		/// Adds children to the element.
		/// </summary>
		/// <param name="NewChildren">New children to add.</param>
		public override void AddChildren(IEnumerable<MarkdownElement> NewChildren)
		{
			MarkdownElement Last;

			foreach (MarkdownElement E in NewChildren)
			{
				if ((Last = this.LastChild) != null && Last is MarkdownElementChildren MarkdownElementChildren && Last.GetType() == E.GetType())
					MarkdownElementChildren.AddChildren(((MarkdownElementChildren)E).Children);
				else
					base.AddChildren((IEnumerable<MarkdownElement>)new MarkdownElement[] { E });
			}
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			this.Export(Output, "DefinitionList");
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
			return new DefinitionList(Document, Children);
		}

	}
}
