using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// META element
	/// </summary>
    public class Meta : HtmlEmptyElement
	{
		/// <summary>
		/// META element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Meta(HtmlElement Parent)
			: base(Parent, "META")
		{
		}
    }
}
