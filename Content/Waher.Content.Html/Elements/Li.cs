﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// LI element
	/// </summary>
    public class Li : HtmlElement
    {
		/// <summary>
		/// LI element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Li(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "LI")
		{
		}
    }
}
