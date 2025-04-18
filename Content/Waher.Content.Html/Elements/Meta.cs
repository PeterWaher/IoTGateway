﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// META element
	/// </summary>
    public class Meta : HtmlEmptyElement
	{
		/// <summary>
		/// META element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Meta(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "META")
		{
		}
    }
}
