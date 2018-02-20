using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// P element
	/// </summary>
    public class P : HtmlElement
    {
		/// <summary>
		/// P element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public P(HtmlElement Parent)
			: base(Parent, "P")
		{
		}
    }
}
