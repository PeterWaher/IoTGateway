using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DIV element
	/// </summary>
    public class Div : HtmlElement
    {
		/// <summary>
		/// DIV element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Div(HtmlElement Parent)
			: base(Parent, "DIV")
		{
		}
    }
}
