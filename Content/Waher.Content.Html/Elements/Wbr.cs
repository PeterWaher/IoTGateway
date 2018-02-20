using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// WBR element
	/// </summary>
    public class Wbr : HtmlEmptyElement
	{
		/// <summary>
		/// WBR element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Wbr(HtmlElement Parent)
			: base(Parent, "WBR")
		{
		}
    }
}
