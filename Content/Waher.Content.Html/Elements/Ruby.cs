using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// RUBY element
	/// </summary>
    public class Ruby : HtmlElement
    {
		/// <summary>
		/// RUBY element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Ruby(HtmlElement Parent)
			: base(Parent, "RUBY")
		{
		}
    }
}
