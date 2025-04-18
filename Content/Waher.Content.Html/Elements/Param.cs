﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// PARAM element
	/// </summary>
    public class Param : HtmlEmptyElement
	{
		/// <summary>
		/// PARAM element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Param(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "PARAM")
		{
		}
    }
}
