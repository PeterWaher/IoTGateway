using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TEXTAREA element
	/// </summary>
    public class TextArea : HtmlElement
    {
		/// <summary>
		/// TEXTAREA element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public TextArea(HtmlElement Parent)
			: base(Parent, "TEXTAREA")
		{
		}
    }
}
