using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// FRAMESET element
	/// </summary>
    public class FrameSet : HtmlElement
    {
		/// <summary>
		/// FRAMESET element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public FrameSet(HtmlElement Parent)
			: base(Parent, "FRAMESET")
		{
		}
    }
}
