using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// NOSCRIPT element
	/// </summary>
    public class NoScript : HtmlElement
    {
		/// <summary>
		/// NOSCRIPT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public NoScript(HtmlElement Parent)
			: base(Parent, "NOSCRIPT")
		{
		}
    }
}
