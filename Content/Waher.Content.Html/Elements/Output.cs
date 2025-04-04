﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// OUTPUT element
	/// </summary>
    public class Output : HtmlElement
    {
		/// <summary>
		/// OUTPUT element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Output(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "OUTPUT")
		{
		}
    }
}
