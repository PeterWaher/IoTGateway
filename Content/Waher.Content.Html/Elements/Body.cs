using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BODY element
	/// </summary>
    public class Body : HtmlElement
    {
		/// <summary>
		/// BODY element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Body(HtmlElement Parent)
			: base(Parent, "BODY")
		{
		}
    }
}
