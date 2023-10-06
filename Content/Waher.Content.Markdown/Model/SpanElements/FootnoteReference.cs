using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model.BlockElements;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Footnote reference
	/// </summary>
	public class FootnoteReference : MarkdownElement
	{
		private readonly string key;
		private bool autoExpand;

		/// <summary>
		/// Footnote reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		public FootnoteReference(MarkdownDocument Document, string Key)
			: base(Document)
		{
			this.key = Key;
			this.autoExpand = false;
		}

		/// <summary>
		/// Footnote key
		/// </summary>
		public string Key => this.key;

		/// <summary>
		/// If the footnote should automatically be expanded when rendered,
		/// if format supports auto-expansion.
		/// </summary>
		public bool AutoExpand
		{
			get => this.autoExpand;
			internal set => this.autoExpand = value;
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override async Task GenerateMarkdown(StringBuilder Output)
		{
			Output.Append("[^");

			if (!this.Document.TryGetFootnote(this.key, out Footnote Footnote))
				Footnote = null;

			if (Guid.TryParse(this.key, out _) && !(Footnote is null))
			{
				StringBuilder sb = new StringBuilder();
				await Footnote.GenerateMarkdown(sb);
				Output.Append(sb.ToString().TrimEnd());
			}
			else
			{
				Output.Append(this.key);

				if (!(Footnote is null))
					Footnote.Referenced = true;
			}

			Output.Append(']');
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			string s;

			if (!this.Document.TryGetFootnote(this.key, out Footnote Footnote))
				Footnote = null;

			if (this.autoExpand && !(Footnote is null))
				await Footnote.GenerateHTML(Output);
			else if (this.Document.TryGetFootnoteNumber(this.key, out int Nr))
			{
				s = Nr.ToString();

				Output.Append("<sup id=\"fnref-");
				Output.Append(s);
				Output.Append("\"><a href=\"#fn-");
				Output.Append(s);
				Output.Append("\" class=\"footnote-ref\">");
				Output.Append(s);
				Output.Append("</a></sup>");

				if (!(Footnote is null))
					Footnote.Referenced = true;
			}
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override Task GeneratePlainText(StringBuilder Output)
		{
			if (this.Document.TryGetFootnote(this.key, out Footnote Footnote))
				Footnote.Referenced = true;

			Output.Append(" [");

			if (this.Document.TryGetFootnoteNumber(this.key, out int Nr))
				Output.Append(Nr.ToString());
			else
				Output.Append(Key);

			Output.Append(']');

			return Task.CompletedTask;
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
			if (!this.Document.TryGetFootnote(this.key, out Footnote Footnote))
				Footnote = null;

			if (this.autoExpand && !(Footnote is null))
				await Footnote.GenerateXAML(Output, TextAlignment);
			else if (this.Document.TryGetFootnoteNumber(this.key, out int Nr))
			{
				XamlSettings Settings = this.Document.Settings.XamlSettings;
				string s;

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

				if (!(Footnote is null))
					Footnote.Referenced = true;
			}
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			if (!this.Document.TryGetFootnote(this.key, out Footnote Footnote))
				Footnote = null;

			if (this.autoExpand && !(Footnote is null))
				await Footnote.GenerateXamarinForms(Output, State);
			else if (this.Document.TryGetFootnoteNumber(this.key, out int Nr))
			{
				bool Bak = State.Superscript;
				State.Superscript = true;

				Paragraph.GenerateXamarinFormsSpan(Output, Nr.ToString(), State);

				State.Superscript = Bak;

				if (!(Footnote is null))
					Footnote.Referenced = true;
			}
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		public override async Task GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State)
		{
			if (!this.Document.TryGetFootnote(this.key, out Footnote Footnote))
				Footnote = null;

			if (this.autoExpand && !(Footnote is null))
				await Footnote.GenerateSmartContractXml(Output, State);
			else if (this.Document.TryGetFootnoteNumber(this.key, out int Nr))
			{
				Output.WriteStartElement("super");
				Output.WriteElementString("text", Nr.ToString());
				Output.WriteEndElement();

				if (!(Footnote is null))
					Footnote.Referenced = true;
			}
		}

		/// <summary>
		/// Generates LaTeX for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		public override async Task GenerateLaTeX(StringBuilder Output)
		{
			if (this.Document.TryGetFootnote(this.key, out Footnote Footnote))
			{
				if (this.autoExpand)
					await Footnote.GenerateLaTeX(Output);
				else
				{
					Output.Append("\\footnote");

					if (this.Document.TryGetFootnoteNumber(this.key, out int Nr))
					{
						Output.Append('[');
						Output.Append(Nr.ToString());
						Output.Append(']');
					}

					Output.Append('{');
					await Footnote.GenerateLaTeX(Output);
					Output.Append('}');
				}
			}
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get
			{
				if (this.autoExpand && this.Document.TryGetFootnote(this.key, out Footnote Footnote))
					return Footnote.InlineSpanElement;
				else
					return true;
			}
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("FootnoteReference");
			Output.WriteAttributeString("key", this.key);

			if (this.Document.TryGetFootnoteNumber(this.key, out int Nr))
				Output.WriteAttributeString("nr", Nr.ToString());

			Output.WriteEndElement();
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is FootnoteReference x &&
				this.key == x.key &&
				this.autoExpand == x.autoExpand &&
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
			int h3 = this.autoExpand.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h1 = ((h1 << 5) + h1) ^ h3;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrFootnoteReference++;
		}
	}
}
