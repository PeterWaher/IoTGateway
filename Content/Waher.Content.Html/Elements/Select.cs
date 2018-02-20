using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SELECT element
	/// </summary>
    public class Select : HtmlElement
    {
		/// <summary>
		/// SELECT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Select(HtmlElement Parent)
			: base(Parent, "SELECT")
		{
		}
    }
}
