using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// EM element
	/// </summary>
    public class Em : HtmlElement
    {
		/// <summary>
		/// EM element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Em(HtmlElement Parent)
			: base(Parent, "EM")
		{
		}
    }
}
