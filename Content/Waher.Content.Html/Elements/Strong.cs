using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// STRONG element
	/// </summary>
    public class Strong : HtmlElement
    {
		/// <summary>
		/// STRONG element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Strong(HtmlElement Parent)
			: base(Parent, "STRONG")
		{
		}
    }
}
