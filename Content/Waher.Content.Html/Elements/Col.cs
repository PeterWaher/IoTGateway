using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// COL element
	/// </summary>
    public class Col : HtmlEmptyElement
    {
		/// <summary>
		/// COL element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Col(HtmlElement Parent)
			: base(Parent, "COL")
		{
		}
    }
}
