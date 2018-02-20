using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BDI element
	/// </summary>
    public class Bdi : HtmlElement
    {
		/// <summary>
		/// BDI element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Bdi(HtmlElement Parent)
			: base(Parent, "BDI")
		{
		}
    }
}
