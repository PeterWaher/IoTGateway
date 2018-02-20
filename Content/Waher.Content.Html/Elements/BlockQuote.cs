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
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public BlockQuote(HtmlElement Parent)
			: base(Parent, "BLOCKQUOTE")
		{
		}
    }
}
