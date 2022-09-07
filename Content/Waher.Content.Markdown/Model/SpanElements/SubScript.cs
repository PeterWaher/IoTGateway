using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override async Task GenerateMarkdown(StringBuilder Output)
		{
			Output.Append('[');
			await base.GenerateMarkdown(Output);
			Output.Append(']');
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			Output.Append("<sub>");

			foreach (MarkdownElement E in this.Children)
				await E.GenerateHTML(Output);

			Output.Append("</sub>");
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			Output.WriteStartElement("Run");
			Output.WriteAttributeString("Typography.Variants", "Subscript");

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
			bool Bak = State.Subscript;
			State.Subscript = true;

			foreach (MarkdownElement E in this.Children)
				await E.GenerateXamarinForms(Output, State);

			State.Subscript = Bak;
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		public override async Task GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State)
		{
			Output.WriteStartElement("sub");

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
				i = nrmScriptLetters.IndexOf(ch);
				if (i < 0)
					Output.Append(ch);
				else
					Output.Append(subScriptLetters[i]);
			}
		}

		/// <summary>
		/// Converts a string to subscript (as far as it goes).
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>String with subscript characters.</returns>
		public static string ToSubscript(string s)
		{
			StringBuilder sb = new StringBuilder();
			int i;

			foreach (char ch in s)
			{
				i = nrmScriptLetters.IndexOf(ch);
				if (i < 0)
					sb.Append(ch);
				else
					sb.Append(subScriptLetters[i]);
			}

			return sb.ToString();
		}

		private const string nrmScriptLetters = "0123456789+-=()aeoxhklmnpstijruv";
		private const string subScriptLetters = "₀₁₂₃₄₅₆₇₈₉₊₋₌₍₎ₐₑₒₓₕₖₗₘₙₚₛₜᵢⱼᵣᵤᵥ";

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
			this.Export(Output, "SubScript");
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
			return new SubScript(Document, Children);
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrSubscript++;
		}

	}
}
