using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// KEYGEN element
	/// </summary>
    public class Keygen : HtmlEmptyElement
    {
		/// <summary>
		/// KEYGEN element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Keygen(HtmlElement Parent)
			: base(Parent, "KEYGEN")
		{
		}
    }
}
