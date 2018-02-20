using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TD element
	/// </summary>
    public class Td : HtmlElement
    {
		/// <summary>
		/// TD element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Td(HtmlElement Parent)
			: base(Parent, "TD")
		{
		}
    }
}
