﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// LABEL element
	/// </summary>
    public class Label : HtmlElement
    {
		/// <summary>
		/// LABEL element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Label(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "LABEL")
		{
		}
    }
}
