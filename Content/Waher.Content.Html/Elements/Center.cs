using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// CENTER element
	/// </summary>
    public class Center : HtmlElement
    {
		/// <summary>
		/// CENTER element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Center(HtmlElement Parent)
			: base(Parent, "CENTER")
		{
		}
    }
}
