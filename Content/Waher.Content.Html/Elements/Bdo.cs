using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// BDO element
	/// </summary>
    public class Bdo : HtmlElement
    {
		/// <summary>
		/// BDO element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Bdo(HtmlElement Parent)
			: base(Parent, "BDO")
		{
		}
    }
}
