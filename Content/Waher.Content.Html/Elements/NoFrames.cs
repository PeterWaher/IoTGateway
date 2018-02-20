using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// NOFRAMES element
	/// </summary>
    public class NoFrames : HtmlElement
    {
		/// <summary>
		/// NOFRAMES element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public NoFrames(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "NOFRAMES")
		{
		}
    }
}
