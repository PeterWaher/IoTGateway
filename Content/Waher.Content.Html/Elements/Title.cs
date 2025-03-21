﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TITLE element
	/// </summary>
    public class Title : HtmlElement
    {
		/// <summary>
		/// TITLE element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Title(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "TITLE")
		{
		}
    }
}
