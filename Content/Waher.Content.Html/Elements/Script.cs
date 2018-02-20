using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SCRIPT element
	/// </summary>
    public class Script : HtmlElement
    {
		/// <summary>
		/// SCRIPT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Script(HtmlElement Parent)
			: base(Parent, "SCRIPT")
		{
		}
    }
}
