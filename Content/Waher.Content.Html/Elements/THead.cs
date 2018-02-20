using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// THEAD element
	/// </summary>
    public class THead : HtmlElement
    {
		/// <summary>
		/// THEAD element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public THead(HtmlElement Parent)
			: base(Parent, "THEAD")
		{
		}
    }
}
