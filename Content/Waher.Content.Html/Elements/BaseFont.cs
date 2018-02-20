using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BASEFONT element
	/// </summary>
    public class BaseFont : HtmlElement
    {
		/// <summary>
		/// BASEFONT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public BaseFont(HtmlElement Parent)
			: base(Parent, "BASEFONT")
		{
		}
    }
}
