using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TEMPLATE element
	/// </summary>
    public class Template : HtmlElement
    {
		/// <summary>
		/// TEMPLATE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Template(HtmlElement Parent)
			: base(Parent, "TEMPLATE")
		{
		}
    }
}
