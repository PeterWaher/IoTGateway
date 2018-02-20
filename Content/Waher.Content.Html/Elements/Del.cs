using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DEL element
	/// </summary>
    public class Del : HtmlElement
    {
		/// <summary>
		/// DEL element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Del(HtmlElement Parent)
			: base(Parent, "DEL")
		{
		}
    }
}
