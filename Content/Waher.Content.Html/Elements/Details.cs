﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DETAILS element
	/// </summary>
    public class Details : HtmlElement
    {
		/// <summary>
		/// DETAILS element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Details(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "DETAILS")
		{
		}
    }
}
