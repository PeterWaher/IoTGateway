using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// COLGROUP element
	/// </summary>
    public class ColGroup : HtmlElement
    {
		/// <summary>
		/// COLGROUP element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public ColGroup(HtmlElement Parent)
			: base(Parent, "COLGROUP")
		{
		}
    }
}
