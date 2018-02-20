using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BASE element
	/// </summary>
    public class Base : HtmlEmptyElement
	{
		/// <summary>
		/// BASE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Base(HtmlElement Parent)
			: base(Parent, "BASE")
		{
		}
    }
}
