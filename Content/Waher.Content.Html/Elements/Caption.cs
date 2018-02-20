using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// CAPTION element
	/// </summary>
    public class Caption : HtmlElement
    {
		/// <summary>
		/// CAPTION element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Caption(HtmlElement Parent)
			: base(Parent, "CAPTION")
		{
		}
    }
}
