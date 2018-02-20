using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// OL element
	/// </summary>
    public class Ol : HtmlElement
    {
		/// <summary>
		/// OL element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Ol(HtmlElement Parent)
			: base(Parent, "OL")
		{
		}
    }
}
