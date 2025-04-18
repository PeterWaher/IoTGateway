﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// RTC element
	/// </summary>
    public class Rtc : HtmlElement
    {
		/// <summary>
		/// RTC element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Rtc(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "RTC")
		{
		}
    }
}
