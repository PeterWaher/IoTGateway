using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// MAIN element
	/// </summary>
    public class Main : HtmlElement
    {
		/// <summary>
		/// MAIN element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Main(HtmlElement Parent)
			: base(Parent, "MAIN")
		{
		}
    }
}
