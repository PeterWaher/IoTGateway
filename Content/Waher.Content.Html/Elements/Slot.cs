﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SLOT element
	/// </summary>
    public class Slot : HtmlElement
    {
		/// <summary>
		/// SLOT element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Slot(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "SLOT")
		{
		}
    }
}
