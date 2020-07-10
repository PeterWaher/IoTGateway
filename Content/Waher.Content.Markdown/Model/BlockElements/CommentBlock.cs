using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a comment block in a markdown document.
	/// </summary>
	public class CommentBlock : BlockElement
	{
		private readonly string[] rows;

		/// <summary>
		/// Represents a comment block in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Rows">Rows</param>
		public CommentBlock(MarkdownDocument Document, string[] Rows)
			: base(Document)
		{
			this.rows = Rows;
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXamarinForms(XmlWriter Output, TextAlignment TextAlignment)
		{
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return false; }
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("CommentBlock");

			foreach (string s in this.rows)
				Output.WriteElementString("Row", s);

			Output.WriteEndElement();
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is CommentBlock x &&
				AreEqual(this.rows, x.rows) &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is CommentBlock x &&
				AreEqual(this.rows, x.rows) &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = GetHashCode(this.rows);

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
