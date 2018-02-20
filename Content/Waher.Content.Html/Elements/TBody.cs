using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TBODY element
	/// </summary>
    public class TBody : HtmlElement
    {
		/// <summary>
		/// TBODY element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public TBody(HtmlElement Parent)
			: base(Parent, "TBODY")
		{
		}
    }
}
