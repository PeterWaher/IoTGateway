using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// B element
	/// </summary>
    public class B : HtmlElement
    {
		/// <summary>
		/// B element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public B(HtmlElement Parent)
			: base(Parent, "B")
		{
		}
    }
}
