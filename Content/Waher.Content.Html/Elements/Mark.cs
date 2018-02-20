using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// MARK element
	/// </summary>
    public class Mark : HtmlElement
    {
		/// <summary>
		/// MARK element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Mark(HtmlElement Parent)
			: base(Parent, "MARK")
		{
		}
    }
}
