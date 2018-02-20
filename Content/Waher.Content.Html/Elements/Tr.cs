using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TR element
	/// </summary>
    public class Tr : HtmlElement
    {
		/// <summary>
		/// TR element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Tr(HtmlElement Parent)
			: base(Parent, "TR")
		{
		}
    }
}
