using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SPACER element
	/// </summary>
    public class Spacer : HtmlElement
    {
		/// <summary>
		/// SPACER element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Spacer(HtmlElement Parent)
			: base(Parent, "SPACER")
		{
		}
    }
}
