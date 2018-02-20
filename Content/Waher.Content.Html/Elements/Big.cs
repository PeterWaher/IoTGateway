using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BIG element
	/// </summary>
    public class Big : HtmlElement
    {
		/// <summary>
		/// BIG element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Big(HtmlElement Parent)
			: base(Parent, "BIG")
		{
		}
    }
}
