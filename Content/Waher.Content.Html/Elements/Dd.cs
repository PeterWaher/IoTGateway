using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DD element
	/// </summary>
    public class Dd : HtmlElement
    {
		/// <summary>
		/// DD element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Dd(HtmlElement Parent)
			: base(Parent, "DD")
		{
		}
    }
}
