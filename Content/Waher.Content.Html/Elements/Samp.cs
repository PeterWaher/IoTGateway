using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SAMP element
	/// </summary>
    public class Samp : HtmlElement
    {
		/// <summary>
		/// SAMP element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Samp(HtmlElement Parent)
			: base(Parent, "SAMP")
		{
		}
    }
}
