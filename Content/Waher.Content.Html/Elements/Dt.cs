using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DT element
	/// </summary>
    public class Dt : HtmlElement
    {
		/// <summary>
		/// DT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Dt(HtmlElement Parent)
			: base(Parent, "DT")
		{
		}
    }
}
