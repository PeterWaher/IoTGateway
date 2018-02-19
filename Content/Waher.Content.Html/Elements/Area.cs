using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// AREA element
	/// </summary>
    public class Area : HtmlEmptyElement
    {
		/// <summary>
		/// AREA element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Area(HtmlElement Parent)
			: base(Parent, "AREA")
		{
		}
    }
}
