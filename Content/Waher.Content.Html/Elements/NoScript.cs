﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// NOSCRIPT element
	/// </summary>
    public class NoScript : HtmlElement
    {
		/// <summary>
		/// NOSCRIPT element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public NoScript(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "NOSCRIPT")
		{
		}
    }
}
