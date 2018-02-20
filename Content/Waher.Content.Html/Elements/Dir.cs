using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DIR element
	/// </summary>
    public class Dir : HtmlElement
    {
		/// <summary>
		/// DIR element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Dir(HtmlElement Parent)
			: base(Parent, "DIR")
		{
		}
    }
}
