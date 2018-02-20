using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// OBJECT element
	/// </summary>
    public class Object : HtmlElement
    {
		/// <summary>
		/// OBJECT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Object(HtmlElement Parent)
			: base(Parent, "OBJECT")
		{
		}
    }
}
