﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// LINK element
	/// </summary>
    public class Link : HtmlEmptyElement
	{
		/// <summary>
		/// LINK element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Link(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "LINK")
		{
		}
    }
}
