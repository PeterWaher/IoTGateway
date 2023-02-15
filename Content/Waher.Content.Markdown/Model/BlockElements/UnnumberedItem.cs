using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model.SpanElements;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents an unnumbered item in an ordered list.
	/// </summary>
	public class UnnumberedItem : BlockElementSingleChild
	{
		private readonly string prefix;

		/// <summary>
		/// Represents an unnumbered item in an ordered list.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Prefix">Prefix, in plain text mode.</param>
		/// <param name="Child">Child element.</param>
		public UnnumberedItem(MarkdownDocument Document, string Prefix, MarkdownElement Child)
			: base(Document, Child)
		{
			this.prefix = Prefix;
		}

		/// <summary>
		/// Prefix, in plain text mode.
		/// </summary>
		public string Prefix => this.prefix;

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override async Task GenerateMarkdown(StringBuilder Output)
		{
			await PrefixedBlock(Output, this.Child, "*\t", "\t");
			Output.AppendLine();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			Output.Append("<li");

			if (!(this.Document.Detail is null))
			{
				if (this.Child is Link Link)
				{
					if (string.Compare(this.Document.Detail.ResourceName, Link.Url, true) == 0)
						Output.Append(" class=\"active\"");
				}
				else if (this.Child is LinkReference LinkReference)
				{
					string Label = LinkReference.Label;
					SpanElements.Multimedia Multimedia = this.Document.GetReference(Label);

					if (!(Multimedia is null) && Multimedia.Items.Length == 1 && string.Compare(Multimedia.Items[0].Url, this.Document.Detail.ResourceName, true) == 0)
						Output.Append(" class=\"active\"");
				}
			}

			Output.Append('>');
			await this.Child.GenerateHTML(Output);
			Output.AppendLine("</li>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override async Task GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.prefix);

			StringBuilder sb = new StringBuilder();
			await this.Child.GeneratePlainText(sb);

			string s = sb.ToString();

			Output.Append(s);

			if (!s.EndsWith("\n"))
				Output.AppendLine();
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			return this.Child.GenerateXAML(Output, TextAlignment);
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			return this.Child.GenerateXamarinForms(Output, State);
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		public override Task GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State)
		{
			return NumberedItem.GenerateSmartContractXmlItem(this.Child, Output, State);
		}

		/// <summary>
		/// Generates LaTeX for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		public override async Task GenerateLaTeX(StringBuilder Output)
		{
			Output.Append("\\item{");

			foreach (MarkdownElement E in this.Children)
				await E.GenerateLaTeX(Output);

			Output.AppendLine("}");
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement => this.Child.InlineSpanElement;

		/// <summary>
		/// Gets margins for content.
		/// </summary>
		/// <param name="TopMargin">Top margin.</param>
		/// <param name="BottomMargin">Bottom margin.</param>
		internal override void GetMargins(out int TopMargin, out int BottomMargin)
		{
			this.Child.GetMargins(out TopMargin, out BottomMargin);
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("UnnumberedItem");
			Output.WriteAttributeString("prefix", this.prefix);
			this.ExportChild(Output);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Child"/>.
		/// </summary>
		/// <param name="Child">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementSingleChild Create(MarkdownElement Child, MarkdownDocument Document)
		{
			return new UnnumberedItem(Document, this.prefix, Child);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is UnnumberedItem x &&
				x.prefix == this.prefix &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is UnnumberedItem x &&
				this.prefix == x.prefix &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.prefix?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrUnnumberedItems++;
			Statistics.NrListItems++;
		}

	}
}
