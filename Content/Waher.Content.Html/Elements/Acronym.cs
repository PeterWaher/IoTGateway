using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// ACRONYM element
	/// </summary>
    public class Acronym : HtmlElement
    {
		/// <summary>
		/// ACRONYM element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Acronym(HtmlElement Parent)
			: base(Parent, "ACRONYM")
		{
		}
    }
}
