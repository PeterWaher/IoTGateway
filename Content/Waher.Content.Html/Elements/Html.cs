using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// HTML element
	/// </summary>
    public class Html : HtmlElement
    {
		/// <summary>
		/// HTML element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Html(HtmlElement Parent)
			: base(Parent, "HTML")
		{
		}
    }
}
