using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// EMBED element
	/// </summary>
    public class Embed : HtmlEmptyElement
	{
		/// <summary>
		/// EMBED element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Embed(HtmlElement Parent)
			: base(Parent, "EMBED")
		{
		}
    }
}
