using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// MAP element
	/// </summary>
    public class Map : HtmlElement
    {
		/// <summary>
		/// MAP element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Map(HtmlElement Parent)
			: base(Parent, "MAP")
		{
		}
    }
}
