using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// STYLE element
	/// </summary>
    public class Style : HtmlElement
    {
		/// <summary>
		/// STYLE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Style(HtmlElement Parent)
			: base(Parent, "STYLE")
		{
		}
    }
}
