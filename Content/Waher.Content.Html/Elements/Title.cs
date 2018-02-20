using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TITLE element
	/// </summary>
    public class Title : HtmlElement
    {
		/// <summary>
		/// TITLE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Title(HtmlElement Parent)
			: base(Parent, "TITLE")
		{
		}
    }
}
