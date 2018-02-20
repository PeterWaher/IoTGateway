using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// IFRAME element
	/// </summary>
    public class IFrame : HtmlElement
    {
		/// <summary>
		/// IFRAME element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public IFrame(HtmlElement Parent)
			: base(Parent, "IFRAME")
		{
		}
    }
}
