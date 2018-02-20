using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// NOBR element
	/// </summary>
    public class NoBr : HtmlElement
    {
		/// <summary>
		/// NOBR element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public NoBr(HtmlElement Parent)
			: base(Parent, "NOBR")
		{
		}
    }
}
