using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DL element
	/// </summary>
    public class Dl : HtmlElement
    {
		/// <summary>
		/// DL element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Dl(HtmlElement Parent)
			: base(Parent, "DL")
		{
		}
    }
}
