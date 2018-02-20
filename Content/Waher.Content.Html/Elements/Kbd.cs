using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// KBD element
	/// </summary>
    public class Kbd : HtmlElement
    {
		/// <summary>
		/// KBD element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Kbd(HtmlElement Parent)
			: base(Parent, "KBD")
		{
		}
    }
}
