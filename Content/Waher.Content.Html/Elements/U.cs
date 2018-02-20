using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// U element
	/// </summary>
    public class U : HtmlElement
    {
		/// <summary>
		/// U element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public U(HtmlElement Parent)
			: base(Parent, "U")
		{
		}
    }
}
