using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// CONTENT element
	/// </summary>
    public class Content : HtmlElement
    {
		/// <summary>
		/// CONTENT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Content(HtmlElement Parent)
			: base(Parent, "CONTENT")
		{
		}
    }
}
