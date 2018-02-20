using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// LEGEND element
	/// </summary>
    public class Legend : HtmlElement
    {
		/// <summary>
		/// LEGEND element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Legend(HtmlElement Parent)
			: base(Parent, "LEGEND")
		{
		}
    }
}
