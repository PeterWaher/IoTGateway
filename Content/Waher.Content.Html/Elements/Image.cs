using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// IMAGE element
	/// </summary>
    public class Image : HtmlElement
    {
		/// <summary>
		/// IMAGE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Image(HtmlElement Parent)
			: base(Parent, "IMAGE")
		{
		}
    }
}
