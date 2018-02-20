using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SUB element
	/// </summary>
    public class Sub : HtmlElement
    {
		/// <summary>
		/// SUB element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Sub(HtmlElement Parent)
			: base(Parent, "SUB")
		{
		}
    }
}
