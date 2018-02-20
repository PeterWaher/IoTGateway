using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SLOT element
	/// </summary>
    public class Slot : HtmlElement
    {
		/// <summary>
		/// SLOT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Slot(HtmlElement Parent)
			: base(Parent, "SLOT")
		{
		}
    }
}
