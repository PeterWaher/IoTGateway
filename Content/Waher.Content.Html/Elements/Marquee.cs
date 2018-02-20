using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// MARQUEE element
	/// </summary>
    public class Marquee : HtmlElement
    {
		/// <summary>
		/// MARQUEE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Marquee(HtmlElement Parent)
			: base(Parent, "MARQUEE")
		{
		}
    }
}
