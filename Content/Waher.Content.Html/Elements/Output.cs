using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// OUTPUT element
	/// </summary>
    public class Output : HtmlElement
    {
		/// <summary>
		/// OUTPUT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Output(HtmlElement Parent)
			: base(Parent, "OUTPUT")
		{
		}
    }
}
