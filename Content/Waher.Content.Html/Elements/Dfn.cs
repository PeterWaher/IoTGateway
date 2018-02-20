using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DFN element
	/// </summary>
    public class Dfn : HtmlElement
    {
		/// <summary>
		/// DFN element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Dfn(HtmlElement Parent)
			: base(Parent, "DFN")
		{
		}
    }
}
