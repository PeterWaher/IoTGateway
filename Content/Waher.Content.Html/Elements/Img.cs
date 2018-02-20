using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// IMG element
	/// </summary>
    public class Img : HtmlEmptyElement
	{
		/// <summary>
		/// IMG element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Img(HtmlElement Parent)
			: base(Parent, "IMG")
		{
		}
    }
}
