using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// Q element
	/// </summary>
    public class Q : HtmlElement
    {
		/// <summary>
		/// Q element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Q(HtmlElement Parent)
			: base(Parent, "Q")
		{
		}
    }
}
