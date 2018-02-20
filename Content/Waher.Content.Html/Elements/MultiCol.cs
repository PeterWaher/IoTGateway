using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// MULTICOL element
	/// </summary>
    public class MultiCol : HtmlElement
    {
		/// <summary>
		/// MULTICOL element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public MultiCol(HtmlElement Parent)
			: base(Parent, "MULTICOL")
		{
		}
    }
}
