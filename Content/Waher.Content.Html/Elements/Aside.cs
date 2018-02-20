using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// ASIDE element
	/// </summary>
    public class Aside : HtmlElement
    {
		/// <summary>
		/// ASIDE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Aside(HtmlElement Parent)
			: base(Parent, "ASIDE")
		{
		}
    }
}
