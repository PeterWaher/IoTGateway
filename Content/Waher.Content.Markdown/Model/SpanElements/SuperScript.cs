using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Super-script text
	/// </summary>
	public class SuperScript : MarkdownElementChildren
	{
		/// <summary>
		/// Super-script text
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		public SuperScript(MarkdownDocument Document, IEnumerable<MarkdownElement> ChildElements)
			: base(Document, ChildElements)
		{
		}

		/// <summary>
		/// Super-script text
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Text">Superscript text.</param>
		public SuperScript(MarkdownDocument Document, string Text)
			: base(Document, new MarkdownElement[] { new InlineText(Document, Text) })
		{
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override async Task GenerateMarkdown(StringBuilder Output)
		{
			Output.Append("^[");
			await base.GenerateMarkdown(Output);
			Output.Append(']');
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			Output.Append("<sup>");

			foreach (MarkdownElement E in this.Children)
				await E.GenerateHTML(Output);

			Output.Append("</sup>");
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			Output.WriteStartElement("Run");
			Output.WriteAttributeString("Typography.Variants", "Superscript");

			foreach (MarkdownElement E in this.Children)
				await E.GenerateXAML(Output, TextAlignment);

			Output.WriteEndElement();
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			bool Bak = State.Superscript;
			State.Superscript = true;

			foreach (MarkdownElement E in this.Children)
				await E.GenerateXamarinForms(Output, State);

			State.Superscript = Bak;
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		public override async Task GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State)
		{
			Output.WriteStartElement("super");

			foreach (MarkdownElement E in this.Children)
				await E.GenerateSmartContractXml(Output, State);

			Output.WriteEndElement();
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override async Task GeneratePlainText(StringBuilder Output)
		{
			StringBuilder sb = new StringBuilder();
			int i;

			await base.GeneratePlainText(sb);

			foreach (char ch in sb.ToString())
			{
				i = normlScriptLetters.IndexOf(ch);
				if (i < 0)
					Output.Append(ch);
				else
					Output.Append(superScriptLetters[i]);
			}
		}

		/// <summary>
		/// Converts a string to superscript (as far as it goes).
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>String with superscript characters.</returns>
		public static string ToSuperscript(string s)
		{
			StringBuilder sb = new StringBuilder();
			int i;

			foreach (char ch in s)
			{
				i = normlScriptLetters.IndexOf(ch);
				if (i < 0)
					sb.Append(ch);
				else
					sb.Append(superScriptLetters[i]);
			}

			return sb.ToString();
		}

		private const string normlScriptLetters = "abcdefghijklmnoprstuvwxyzABDEGHIJKLMNOPRTUW0123456789+-=()";
		private const string superScriptLetters = "ᵃᵇᶜᵈᵉᶠᵍʰⁱʲᵏˡᵐⁿᵒᵖʳˢᵗᵘᵛʷˣʸᶻᴬᴮᴰᴱᴳᴴᴵᴶᴷᴸᴹᴺᴼᴾᴿᵀᵁᵂ⁰¹²³⁴⁵⁶⁷⁸⁹⁺⁻⁼⁽⁾";

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement => true;

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			this.Export(Output, "SuperScript");
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
			return new SuperScript(Document, Children);
		}

	}
}
