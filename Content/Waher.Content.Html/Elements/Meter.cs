using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// METER element
	/// </summary>
    public class Meter : HtmlElement
    {
		/// <summary>
		/// METER element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Meter(HtmlElement Parent)
			: base(Parent, "METER")
		{
		}
    }
}
