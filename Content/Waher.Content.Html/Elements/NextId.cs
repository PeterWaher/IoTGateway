using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// NEXTID element
	/// </summary>
    public class NextId : HtmlElement
    {
		/// <summary>
		/// NEXTID element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public NextId(HtmlElement Parent)
			: base(Parent, "NEXTID")
		{
		}
    }
}
