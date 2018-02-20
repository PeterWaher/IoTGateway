using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// OPTGROUP element
	/// </summary>
    public class OptGroup : HtmlElement
    {
		/// <summary>
		/// OPTGROUP element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public OptGroup(HtmlElement Parent)
			: base(Parent, "OPTGROUP")
		{
		}
    }
}
