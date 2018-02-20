using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// CANVAS element
	/// </summary>
    public class Canvas : HtmlElement
    {
		/// <summary>
		/// CANVAS element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Canvas(HtmlElement Parent)
			: base(Parent, "CANVAS")
		{
		}
    }
}
