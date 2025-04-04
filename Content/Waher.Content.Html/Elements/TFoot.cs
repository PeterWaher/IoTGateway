﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TFOOT element
	/// </summary>
    public class TFoot : HtmlElement
    {
		/// <summary>
		/// TFOOT element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public TFoot(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "TFOOT")
		{
		}
    }
}
