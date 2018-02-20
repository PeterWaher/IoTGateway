using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// FIGURE element
	/// </summary>
    public class Figure : HtmlElement
    {
		/// <summary>
		/// FIGURE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Figure(HtmlElement Parent)
			: base(Parent, "FIGURE")
		{
		}
    }
}
