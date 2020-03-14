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
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return this.GetType().Equals(obj.GetType());
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return this.GetType().GetHashCode();
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

			string s = Temp.ToString().Replace("\r\n", "\n").Replace('\r', '\n');
			string[] Rows = s.Split('\n');
			int i, c = Rows.Length;

			if (c > 0 && string.IsNullOrEmpty(Rows[c - 1]))
				c--;

			for (i = 0; i < c; i++)
			{
				Output.Append(PrefixFirstRow);
				Output.AppendLine(Rows[i]);
				PrefixFirstRow = PrefixNextRows;
			}
		}

		/// <summary>
		/// Checks if two typed arrays are equal
		/// </summary>
		/// <param name="Items1">First array</param>
		/// <param name="Items2">Second array</param>
		/// <returns>If arrays are equal</returns>
		protected static bool AreEqual(Array Items1, Array Items2)
		{
			int i, c = Items1.Length;
			if (Items2.Length != c)
				return false;

			for (i = 0; i < c; i++)
			{
				if (!Items1.GetValue(i)?.Equals(Items2.GetValue(i)) ?? Items2.GetValue(i) is null)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates a hash value on an array.
		/// </summary>
		/// <param name="Items">Array</param>
		/// <returns>Hash Code</returns>
		protected static int GetHashCode(Array Items)
		{
			int h1 = 0;
			int h2;

			foreach (object Item in Items)
			{
				h2 = Item?.GetHashCode() ?? 0;
				h1 = ((h1 << 5) + h1) ^ h2;
			}

			return h1;
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder Result = new StringBuilder();
			this.GenerateMarkdown(Result);
			return Result.ToString();
		}

	}
}
