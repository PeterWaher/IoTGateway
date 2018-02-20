using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SMALL element
	/// </summary>
    public class Small : HtmlElement
    {
		/// <summary>
		/// SMALL element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Small(HtmlElement Parent)
			: base(Parent, "SMALL")
		{
		}
    }
}
