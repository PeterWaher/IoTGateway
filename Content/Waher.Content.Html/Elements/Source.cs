using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SOURCE element
	/// </summary>
    public class Source : HtmlEmptyElement
	{
		/// <summary>
		/// SOURCE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Source(HtmlElement Parent)
			: base(Parent, "SOURCE")
		{
		}
    }
}
