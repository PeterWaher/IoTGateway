﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// LEGEND element
	/// </summary>
    public class Legend : HtmlElement
    {
		/// <summary>
		/// LEGEND element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Legend(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "LEGEND")
		{
		}
    }
}
