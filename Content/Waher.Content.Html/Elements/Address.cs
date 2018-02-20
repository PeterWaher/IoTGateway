using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// ADDRESS element
	/// </summary>
    public class Address : HtmlElement
    {
		/// <summary>
		/// ADDRESS element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Address(HtmlElement Parent)
			: base(Parent, "ADDRESS")
		{
		}
    }
}
