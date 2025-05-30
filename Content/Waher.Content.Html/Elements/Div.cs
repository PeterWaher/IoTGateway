﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DIV element
	/// </summary>
    public class Div : HtmlElement
    {
		/// <summary>
		/// DIV element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Div(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "DIV")
		{
		}
    }
}
