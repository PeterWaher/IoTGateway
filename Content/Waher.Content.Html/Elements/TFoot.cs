using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TFOOT element
	/// </summary>
    public class TFoot : HtmlElement
    {
		/// <summary>
		/// TFOOT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public TFoot(HtmlElement Parent)
			: base(Parent, "TFOOT")
		{
		}
    }
}
