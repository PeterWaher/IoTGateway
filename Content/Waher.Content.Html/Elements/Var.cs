using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// VAR element
	/// </summary>
    public class Var : HtmlElement
    {
		/// <summary>
		/// VAR element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Var(HtmlElement Parent)
			: base(Parent, "VAR")
		{
		}
    }
}
