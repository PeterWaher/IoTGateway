using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// AUDIO element
	/// </summary>
    public class Audio : HtmlElement
    {
		/// <summary>
		/// AUDIO element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Audio(HtmlElement Parent)
			: base(Parent, "AUDIO")
		{
		}
    }
}
