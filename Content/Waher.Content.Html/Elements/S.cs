using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// S element
	/// </summary>
    public class S : HtmlElement
    {
		/// <summary>
		/// S element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public S(HtmlElement Parent)
			: base(Parent, "S")
		{
		}
    }
}
