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
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public NoFrames(HtmlElement Parent)
			: base(Parent, "NOFRAMES")
		{
		}
    }
}
