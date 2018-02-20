using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TIME element
	/// </summary>
    public class Time : HtmlElement
    {
		/// <summary>
		/// TIME element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Time(HtmlElement Parent)
			: base(Parent, "TIME")
		{
		}
    }
}
