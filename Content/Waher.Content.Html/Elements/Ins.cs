using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// INS element
	/// </summary>
    public class Ins : HtmlElement
    {
		/// <summary>
		/// INS element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Ins(HtmlElement Parent)
			: base(Parent, "INS")
		{
		}
    }
}
