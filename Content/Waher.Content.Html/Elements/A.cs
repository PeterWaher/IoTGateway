using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// A element
	/// </summary>
    public class A : HtmlElement
    {
		/// <summary>
		/// A element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public A(HtmlElement Parent)
			: base(Parent, "A")
		{
		}
    }
}
