﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// METER element
	/// </summary>
    public class Meter : HtmlElement
    {
		/// <summary>
		/// METER element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Meter(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "METER")
		{
		}
    }
}
