﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// COMMAND element
	/// </summary>
    public class Command : HtmlElement
    {
		/// <summary>
		/// COMMAND element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Command(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "COMMAND")
		{
		}
    }
}
