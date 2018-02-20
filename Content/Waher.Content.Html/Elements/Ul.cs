using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// UL element
	/// </summary>
    public class Ul : HtmlElement
    {
		/// <summary>
		/// UL element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Ul(HtmlElement Parent)
			: base(Parent, "UL")
		{
		}
    }
}
