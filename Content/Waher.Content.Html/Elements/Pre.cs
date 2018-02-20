using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// PRE element
	/// </summary>
    public class Pre : HtmlElement
    {
		/// <summary>
		/// PRE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Pre(HtmlElement Parent)
			: base(Parent, "PRE")
		{
		}
    }
}
