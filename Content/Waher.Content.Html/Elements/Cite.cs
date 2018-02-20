using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// CITE element
	/// </summary>
    public class Cite : HtmlElement
    {
		/// <summary>
		/// CITE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Cite(HtmlElement Parent)
			: base(Parent, "CITE")
		{
		}
    }
}
