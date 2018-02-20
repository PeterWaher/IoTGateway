using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SHADOW element
	/// </summary>
    public class Shadow : HtmlElement
    {
		/// <summary>
		/// SHADOW element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Shadow(HtmlElement Parent)
			: base(Parent, "SHADOW")
		{
		}
    }
}
