﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TR element
	/// </summary>
    public class Tr : HtmlElement
    {
		/// <summary>
		/// TR element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Tr(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "TR")
		{
		}
    }
}
