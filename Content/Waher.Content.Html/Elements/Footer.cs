using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// FOOTER element
	/// </summary>
    public class Footer : HtmlElement
    {
		/// <summary>
		/// FOOTER element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Footer(HtmlElement Parent)
			: base(Parent, "FOOTER")
		{
		}
    }
}
