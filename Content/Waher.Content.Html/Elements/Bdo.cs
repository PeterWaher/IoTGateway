﻿namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BDO element
	/// </summary>
    public class Bdo : HtmlElement
    {
		/// <summary>
		/// BDO element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Bdo(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "BDO")
		{
		}
    }
}
