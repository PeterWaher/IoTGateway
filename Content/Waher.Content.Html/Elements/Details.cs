using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DETAILS element
	/// </summary>
    public class Details : HtmlElement
    {
		/// <summary>
		/// DETAILS element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Details(HtmlElement Parent)
			: base(Parent, "DETAILS")
		{
		}
    }
}
