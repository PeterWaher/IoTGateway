using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TH element
	/// </summary>
    public class Th : HtmlElement
    {
		/// <summary>
		/// TH element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Th(HtmlElement Parent)
			: base(Parent, "TH")
		{
		}
    }
}
