﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// CODE element
	/// </summary>
    public class Code : HtmlElement
    {
		/// <summary>
		/// CODE element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Code(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "CODE")
		{
		}
    }
}
