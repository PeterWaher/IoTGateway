﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// EMBED element
	/// </summary>
    public class Embed : HtmlEmptyElement
	{
		/// <summary>
		/// EMBED element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Embed(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "EMBED")
		{
		}
    }
}
