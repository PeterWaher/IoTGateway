using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SPAN element
	/// </summary>
    public class Span : HtmlElement
    {
		/// <summary>
		/// SPAN element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Span(HtmlElement Parent)
			: base(Parent, "SPAN")
		{
		}
    }
}
