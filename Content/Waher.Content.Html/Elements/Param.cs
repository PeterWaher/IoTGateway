using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// PARAM element
	/// </summary>
    public class Param : HtmlEmptyElement
    {
		/// <summary>
		/// PARAM element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Param(HtmlElement Parent)
			: base(Parent, "PARAM")
		{
		}
    }
}
