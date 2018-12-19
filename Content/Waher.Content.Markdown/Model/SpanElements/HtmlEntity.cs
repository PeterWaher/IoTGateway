using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Represents an HTML entity.
	/// </summary>
	public class HtmlEntity : MarkdownElement
	{
		private string entity;

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
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append('&');
			Output.Append(this.entity);
			Output.Append(';');
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
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			string s = Html.HtmlEntity.EntityToCharacter(this.entity);
			if (s is null)
				Output.WriteRaw("&" + this.entity + ";");
			else
				Output.WriteValue(s);
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
	}
}
