using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BR element
	/// </summary>
    public class Br : HtmlEmptyElement
    {
		/// <summary>
		/// BR element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Br(HtmlElement Parent)
			: base(Parent, "BR")
		{
		}
    }
}
