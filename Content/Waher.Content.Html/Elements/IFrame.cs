﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// IFRAME element
	/// </summary>
    public class IFrame : HtmlElement
    {
		/// <summary>
		/// IFRAME element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public IFrame(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "IFRAME")
		{
		}
    }
}
