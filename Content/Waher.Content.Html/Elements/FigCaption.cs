using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// FIGCAPTION element
	/// </summary>
    public class FigCaption : HtmlElement
    {
		/// <summary>
		/// FIGCAPTION element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public FigCaption(HtmlElement Parent)
			: base(Parent, "FIGCAPTION")
		{
		}
    }
}
