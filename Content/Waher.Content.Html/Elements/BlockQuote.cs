using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BLOCKQUOTE element
	/// </summary>
    public class BlockQuote : HtmlElement
    {
		/// <summary>
		/// BLOCKQUOTE element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public BlockQuote(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "BLOCKQUOTE")
		{
		}
    }
}
