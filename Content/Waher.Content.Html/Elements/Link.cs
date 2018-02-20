using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// LINK element
	/// </summary>
    public class Link : HtmlEmptyElement
	{
		/// <summary>
		/// LINK element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Link(HtmlElement Parent)
			: base(Parent, "LINK")
		{
		}
    }
}
