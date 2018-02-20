using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// FORM element
	/// </summary>
    public class Form : HtmlElement
    {
		/// <summary>
		/// FORM element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Form(HtmlElement Parent)
			: base(Parent, "FORM")
		{
		}
    }
}
