using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// PICTURE element
	/// </summary>
    public class Picture : HtmlElement
    {
		/// <summary>
		/// PICTURE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Picture(HtmlElement Parent)
			: base(Parent, "PICTURE")
		{
		}
    }
}
