using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TT element
	/// </summary>
    public class Tt : HtmlElement
    {
		/// <summary>
		/// TT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Tt(HtmlElement Parent)
			: base(Parent, "TT")
		{
		}
    }
}
