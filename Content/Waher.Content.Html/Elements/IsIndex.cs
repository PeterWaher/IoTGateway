﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// ISINDEX element
	/// </summary>
    public class IsIndex : HtmlElement
    {
		/// <summary>
		/// ISINDEX element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public IsIndex(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "ISINDEX")
		{
		}
    }
}
