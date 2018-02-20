using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BUTTON element
	/// </summary>
    public class Button : HtmlElement
    {
		/// <summary>
		/// BUTTON element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Button(HtmlElement Parent)
			: base(Parent, "BUTTON")
		{
		}
    }
}
