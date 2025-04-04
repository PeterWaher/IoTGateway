﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BUTTON element
	/// </summary>
    public class Button : HtmlElement
    {
		/// <summary>
		/// BUTTON element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Button(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "BUTTON")
		{
		}
    }
}
