using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model.Atoms;
using Waher.Content.Markdown.Model.BlockElements;
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
			get => this.value;
			internal set => this.value = value;
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override Task GenerateMarkdown(StringBuilder Output)
		{
			Output.Append(MarkdownDocument.Encode(this.value));
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override Task GenerateHTML(StringBuilder Output)
		{
			Output.Append(XML.HtmlValueEncode(this.value));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override Task GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.value);
		
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.value;
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			Output.WriteValue(this.value);
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			Paragraph.GenerateXamarinFormsSpan(Output, this.value, State);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		public override Task GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State)
		{
			Output.WriteElementString("text", this.value);
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates LaTeX for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		public override Task GenerateLaTeX(StringBuilder Output)
		{
			Output.AppendLine(EscapeLaTeX(this.value));
			return Task.CompletedTask;
		}

		/// <summary>
		/// Escapes text for output in a LaTeX document.
		/// </summary>
		/// <param name="s">String to escape.</param>
		/// <returns>Escaped string.</returns>
		public static string EscapeLaTeX(string s)
		{
			return CommonTypes.Escape(s, latexCharactersToEscape, latexCharacterEscapes);
		}

		private static readonly char[] latexCharactersToEscape = new char[] { '\\', '#', '$', '%', '&', '{', '}' };
		private static readonly string[] latexCharacterEscapes = new string[] { "\\,", "\\#", "\\$", "\\%", "\\&", "\\{", "\\}" };

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
			LinkedList<Atom> Result = new LinkedList<Atom>();

			foreach (char ch in this.value)
				Result.AddLast(new InlineTextCharacter(this.Document, this, ch));

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
			return new InlineText(Document, Text);
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrInlineText++;
		}
	}
}
