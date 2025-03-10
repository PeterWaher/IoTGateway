﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// FONT element
	/// </summary>
    public class Font : HtmlElement
    {
		/// <summary>
		/// FONT element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Font(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "FONT")
		{
		}
    }
}
