using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// FRAME element
	/// </summary>
    public class Frame : HtmlElement
    {
		/// <summary>
		/// FRAME element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Frame(HtmlElement Parent)
			: base(Parent, "FRAME")
		{
		}
    }
}
