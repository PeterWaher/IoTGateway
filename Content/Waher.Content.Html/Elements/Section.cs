using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SECTION element
	/// </summary>
    public class Section : HtmlElement
    {
		/// <summary>
		/// SECTION element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Section(HtmlElement Parent)
			: base(Parent, "SECTION")
		{
		}
    }
}
