using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// LISTING element
	/// </summary>
    public class Listing : HtmlElement
    {
		/// <summary>
		/// LISTING element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Listing(HtmlElement Parent)
			: base(Parent, "LISTING")
		{
		}
    }
}
