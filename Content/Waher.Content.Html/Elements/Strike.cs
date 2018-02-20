using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// STRIKE element
	/// </summary>
    public class Strike : HtmlElement
    {
		/// <summary>
		/// STRIKE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Strike(HtmlElement Parent)
			: base(Parent, "STRIKE")
		{
		}
    }
}
