using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Content.Html
{
	/// <summary>
	/// In-line text.
	/// </summary>
    public class HtmlText : HtmlNode
    {
		private string text;
		private bool? isWhiteSpace;

		/// <summary>
		/// In-line text.
		/// </summary>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Text">Text.</param>
		public HtmlText(HtmlNode Parent, string Text)
			: base(Parent)
		{
			this.text = Text;
		}

		/// <summary>
		/// Inline text.
		/// </summary>
		public string InlineText
		{
			get { return this.text; }
		}

		/// <summary>
		/// If the text consists only of white-space.
		/// </summary>
		public bool IsWhiteSpace
		{
			get
			{
				if (!this.isWhiteSpace.HasValue)
					this.isWhiteSpace = string.IsNullOrEmpty(this.text.Trim());

				return this.isWhiteSpace.Value;
			}
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.text;
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteValue(this.text);
		}
	}
}
