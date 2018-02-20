using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// LI element
	/// </summary>
    public class Li : HtmlElement
    {
		/// <summary>
		/// LI element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Li(HtmlElement Parent)
			: base(Parent, "LI")
		{
		}
    }
}
