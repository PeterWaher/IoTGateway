using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// APPLET element
	/// </summary>
    public class Applet : HtmlElement
    {
		/// <summary>
		/// APPLET element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Applet(HtmlElement Parent)
			: base(Parent, "APPLET")
		{
		}
    }
}
