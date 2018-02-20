using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// ELEMENT element
	/// </summary>
    public class Element : HtmlElement
    {
		/// <summary>
		/// ELEMENT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Element(HtmlElement Parent)
			: base(Parent, "ELEMENT")
		{
		}
    }
}
