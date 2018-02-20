using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// CODE element
	/// </summary>
    public class Code : HtmlElement
    {
		/// <summary>
		/// CODE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Code(HtmlElement Parent)
			: base(Parent, "CODE")
		{
		}
    }
}
