using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// MARK element
	/// </summary>
    public class Mark : HtmlElement
    {
		/// <summary>
		/// MARK element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public Mark(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "MARK")
		{
		}
    }
}
