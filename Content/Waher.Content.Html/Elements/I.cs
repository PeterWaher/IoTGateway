using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// I element
	/// </summary>
    public class I : HtmlElement
    {
		/// <summary>
		/// I element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public I(HtmlElement Parent)
			: base(Parent, "I")
		{
		}
    }
}
