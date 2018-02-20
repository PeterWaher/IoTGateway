using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// MENUITEM element
	/// </summary>
    public class MenuItem : HtmlElement
    {
		/// <summary>
		/// MENUITEM element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public MenuItem(HtmlElement Parent)
			: base(Parent, "MENUITEM")
		{
		}
    }
}
