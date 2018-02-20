using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// LABEL element
	/// </summary>
    public class Label : HtmlElement
    {
		/// <summary>
		/// LABEL element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Label(HtmlElement Parent)
			: base(Parent, "LABEL")
		{
		}
    }
}
