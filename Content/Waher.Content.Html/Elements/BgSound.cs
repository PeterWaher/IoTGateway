using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BGSOUND element
	/// </summary>
    public class BgSound : HtmlElement
    {
		/// <summary>
		/// BGSOUND element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public BgSound(HtmlElement Parent)
			: base(Parent, "BGSOUND")
		{
		}
    }
}
