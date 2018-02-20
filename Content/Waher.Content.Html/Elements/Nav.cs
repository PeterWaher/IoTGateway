using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// NAV element
	/// </summary>
    public class Nav : HtmlElement
    {
		/// <summary>
		/// NAV element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Nav(HtmlElement Parent)
			: base(Parent, "NAV")
		{
		}
    }
}
