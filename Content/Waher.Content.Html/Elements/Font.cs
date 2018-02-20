using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// FONT element
	/// </summary>
    public class Font : HtmlElement
    {
		/// <summary>
		/// FONT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Font(HtmlElement Parent)
			: base(Parent, "FONT")
		{
		}
    }
}
