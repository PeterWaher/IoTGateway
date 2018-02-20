using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// ISINDEX element
	/// </summary>
    public class IsIndex : HtmlElement
    {
		/// <summary>
		/// ISINDEX element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public IsIndex(HtmlElement Parent)
			: base(Parent, "ISINDEX")
		{
		}
    }
}
