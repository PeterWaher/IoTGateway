using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BLINK element
	/// </summary>
    public class BLink : HtmlElement
    {
		/// <summary>
		/// BLINK element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public BLink(HtmlElement Parent)
			: base(Parent, "BLINK")
		{
		}
    }
}
