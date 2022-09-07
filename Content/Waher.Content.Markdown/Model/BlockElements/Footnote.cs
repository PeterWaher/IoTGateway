using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Footnote
	/// </summary>
	public class Footnote : BlockElementChildren
	{
		private readonly string key;

		/// <summary>
		/// Footnote reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="Children">Child elements.</param>
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
		/// <param name="Children">Child elements.</param>
		public Footnote(MarkdownDocument Document, string Key, params MarkdownElement[] Children)
			: base(Document, Children)
		{
			this.key = Key;
		}

		/// <summary>
		/// Footnote key
		/// </summary>
		public string Key => this.key;

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GenerateHTML(Output);
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override async Task GenerateMarkdown(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GenerateMarkdown(Output);
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override async Task GeneratePlainText(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GeneratePlainText(Output);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.key;
		}

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
		internal override bool InlineSpanElement
		{
			get
			{
				if (this.HasOneChild)
					return this.FirstChild.InlineSpanElement;
				else
					return false;
			}
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("Footnote");

			if (this.Document.TryGetFootnoteNumber(this.key, out int Nr))
				Output.WriteAttributeString("nr", Nr.ToString());
			else
				Output.WriteAttributeString("key", this.key);

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
			return new Footnote(Document, this.key, Children);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is Footnote x &&
				x.key == this.key &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Footnote x &&
				this.key == x.key &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.key?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrFootnotes++;
		}

	}
}
