using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// INPUT element
	/// </summary>
    public class Input : HtmlEmptyElement
	{
		/// <summary>
		/// INPUT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Input(HtmlElement Parent)
			: base(Parent, "INPUT")
		{
		}
    }
}
