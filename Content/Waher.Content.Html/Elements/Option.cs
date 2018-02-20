using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// OPTION element
	/// </summary>
    public class Option : HtmlElement
    {
		/// <summary>
		/// OPTION element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Option(HtmlElement Parent)
			: base(Parent, "OPTION")
		{
		}
    }
}
