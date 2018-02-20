using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// HR element
	/// </summary>
    public class Hr : HtmlEmptyElement
	{
		/// <summary>
		/// HR element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Hr(HtmlElement Parent)
			: base(Parent, "HR")
		{
		}
    }
}
