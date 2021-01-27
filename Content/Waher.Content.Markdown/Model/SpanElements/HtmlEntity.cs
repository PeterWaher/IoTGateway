using System;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Represents an HTML entity.
	/// </summary>
	public class HtmlEntity : MarkdownElement
	{
		private readonly string entity;

		/// <summary>
		/// Represents an HTML entity.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Entity">HTML Entity.</param>
		public HtmlEntity(MarkdownDocument Document, string Entity)
			: base(Document)
		{
			this.entity = Entity;
		}

		/// <summary>
		/// HTML Entity
		/// </summary>
		public string Entity
		{
			get { return this.entity; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			this.GenerateHTML(Output);
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			if (this.Document.Settings.HtmlSettings?.XmlEntitiesOnly ?? true)
			{
				switch (this.entity)
				{
					case "quot":
					case "amp":
					case "apos":
					case "lt":
					case "gt":
						Output.Append('&');
						Output.Append(this.entity);
						Output.Append(';');
						break;

					case "QUOT":
					case "AMP":
					case "LT":
					case "GT":
						Output.Append('&');
						Output.Append(this.entity.ToLower());
						Output.Append(';');
						break;

					default:
						string s = Html.HtmlEntity.EntityToCharacter(this.entity);
						if (!string.IsNullOrEmpty(s))
							Output.Append(s);
						break;
				}
			}
			else
			{
				Output.Append('&');
				Output.Append(this.entity);
				Output.Append(';');
			}
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			string s = Html.HtmlEntity.EntityToCharacter(this.entity);
			if (s is null)
				this.GenerateHTML(Output);
			else
				Output.Append(s);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return "&" + this.entity + ";";
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			string s = Html.HtmlEntity.EntityToCharacter(this.entity);
			if (s is null)
				Output.WriteRaw("&" + this.entity + ";");
			else
				Output.WriteValue(s);
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXamarinForms(XmlWriter Output, TextAlignment TextAlignment)
		{
			InlineText.GenerateInlineFormattedTextXamarinForms(Output, this);
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
			Output.WriteElementString("HtmlEntity", this.entity);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is HtmlEntity x &&
				this.entity == x.entity &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.entity?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
