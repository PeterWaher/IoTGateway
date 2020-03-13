using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Abstract base class for all markdown elements.
	/// </summary>
	public abstract class MarkdownElement
	{
		private readonly MarkdownDocument document;

		/// <summary>
		/// Abstract base class for all markdown elements.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		public MarkdownElement(MarkdownDocument Document)
		{
			this.document = Document;
		}

		/// <summary>
		/// Markdown document.
		/// </summary>
		public MarkdownDocument Document
		{
			get { return this.document; }
		}

		/// <summary>
		/// If the element is a block element.
		/// </summary>
		public virtual bool IsBlockElement
		{
			get { return false; }
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public virtual bool SameMetaData(MarkdownElement E)
		{
			return this.GetType() == E.GetType();
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public abstract void GenerateMarkdown(StringBuilder Output);

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public abstract void GenerateHTML(StringBuilder Output);

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public abstract void GeneratePlainText(StringBuilder Output);

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public abstract void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment);

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		internal virtual bool OutsideParagraph
		{
			get { return false; }
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal abstract bool InlineSpanElement
		{
			get;
		}

		/// <summary>
		/// Baseline alignment
		/// </summary>
		internal virtual string BaselineAlignment
		{
			get { return "Center"; }
		}

		/// <summary>
		/// Gets margins for content.
		/// </summary>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TopMargin">Top margin.</param>
		/// <param name="BottomMargin">Bottom margin.</param>
		internal virtual void GetMargins(XamlSettings Settings, out int TopMargin, out int BottomMargin)
		{
			if (this.InlineSpanElement && !this.OutsideParagraph)
			{
				TopMargin = 0;
				BottomMargin = 0;
			}
			else
			{
				TopMargin = Settings.ParagraphMarginTop;
				BottomMargin = Settings.ParagraphMarginBottom;
			}
		}

		/// <summary>
		/// Loops through all child-elements for the element.
		/// </summary>
		/// <param name="Callback">Method called for each one of the elements.</param>
		/// <param name="State">State object passed on to the callback method.</param>
		/// <returns>If the operation was completed.</returns>
		public virtual bool ForEach(MarkdownElementHandler Callback, object State)
		{
			return Callback(this, State);
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public abstract void Export(XmlWriter Output);

		/// <summary>
		/// Prefixes a block of markdown.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		/// <param name="Child">Child element.</param>
		/// <param name="Prefix">Block prefix</param>
		protected static void PrefixedBlock(StringBuilder Output, MarkdownElement Child, string Prefix)
		{
			PrefixedBlock(Output, Child, Prefix, Prefix);
		}

		/// <summary>
		/// Prefixes a block of markdown.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		/// <param name="Children">Child elements.</param>
		/// <param name="Prefix">Block prefix</param>
		protected static void PrefixedBlock(StringBuilder Output, IEnumerable<MarkdownElement> Children, string Prefix)
		{
			PrefixedBlock(Output, Children, Prefix, Prefix);
		}

		/// <summary>
		/// Prefixes a block of markdown.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		/// <param name="Child">Child element.</param>
		/// <param name="PrefixFirstRow">Prefix, for first row.</param>
		/// <param name="PrefixNextRows">Prefix, for the rest of the rows, if any.</param>
		protected static void PrefixedBlock(StringBuilder Output, MarkdownElement Child, string PrefixFirstRow, string PrefixNextRows)
		{
			PrefixedBlock(Output, new MarkdownElement[] { Child }, PrefixFirstRow, PrefixNextRows);
		}

		/// <summary>
		/// Prefixes a block of markdown.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		/// <param name="Children">Child elements.</param>
		/// <param name="PrefixFirstRow">Prefix, for first row.</param>
		/// <param name="PrefixNextRows">Prefix, for the rest of the rows, if any.</param>
		protected static void PrefixedBlock(StringBuilder Output, IEnumerable<MarkdownElement> Children,
			string PrefixFirstRow, string PrefixNextRows)
		{
			StringBuilder Temp = new StringBuilder();

			foreach (MarkdownElement E in Children)
				E.GenerateMarkdown(Temp);

			string s = Temp.ToString();

			foreach (string Row in s.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
			{
				Output.Append(PrefixFirstRow);
				Output.AppendLine(Row);
				PrefixFirstRow = PrefixNextRows;
			}
		}

	}
}
