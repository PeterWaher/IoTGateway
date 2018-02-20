using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// HEADER element
	/// </summary>
    public class Header : HtmlElement
    {
		/// <summary>
		/// HEADER element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Header(HtmlElement Parent)
			: base(Parent, "HEADER")
		{
		}
    }
}
