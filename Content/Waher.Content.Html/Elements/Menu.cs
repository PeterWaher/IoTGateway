using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// MENU element
	/// </summary>
    public class Menu : HtmlElement
    {
		/// <summary>
		/// MENU element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Menu(HtmlElement Parent)
			: base(Parent, "MENU")
		{
		}
    }
}
