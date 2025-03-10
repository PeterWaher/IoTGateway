﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// IMG element
	/// </summary>
    public class Img : HtmlEmptyElement
	{
		/// <summary>
		/// IMG element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Img(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "IMG")
		{
		}
    }
}
