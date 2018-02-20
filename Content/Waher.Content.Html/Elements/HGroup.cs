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
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public HGroup(HtmlElement Parent)
			: base(Parent, "HGROUP")
		{
		}
    }
}
