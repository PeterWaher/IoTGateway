using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// ABBR element
	/// </summary>
    public class Abbr : HtmlElement
    {
		/// <summary>
		/// ABBR element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Abbr(HtmlElement Parent)
			: base(Parent, "ABBR")
		{
		}
    }
}
