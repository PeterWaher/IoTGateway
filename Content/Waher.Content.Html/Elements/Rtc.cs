using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// RTC element
	/// </summary>
    public class Rtc : HtmlElement
    {
		/// <summary>
		/// RTC element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Rtc(HtmlElement Parent)
			: base(Parent, "RTC")
		{
		}
    }
}
