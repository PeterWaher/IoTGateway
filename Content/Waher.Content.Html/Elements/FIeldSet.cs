using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// FIELDSET element
	/// </summary>
    public class FieldSet : HtmlElement
    {
		/// <summary>
		/// FIELDSET element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public FieldSet(HtmlElement Parent)
			: base(Parent, "FIELDSET")
		{
		}
    }
}
