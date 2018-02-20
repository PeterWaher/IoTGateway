using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// HEAD element
	/// </summary>
    public class Head : HtmlElement
    {
		/// <summary>
		/// HEAD element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Head(HtmlElement Parent)
			: base(Parent, "HEAD")
		{
		}
    }
}
