using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// HGROUP element
	/// </summary>
    public class HGroup : HtmlElement
    {
		/// <summary>
		/// HGROUP element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		public HGroup(HtmlDocument Document, HtmlElement Parent, int StartPosition)
			: base(Document, Parent, StartPosition, "HGROUP")
		{
		}
    }
}
