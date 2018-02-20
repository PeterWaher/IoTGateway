using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// XMP element
	/// </summary>
    public class Xmp : HtmlElement
    {
		/// <summary>
		/// XMP element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Xmp(HtmlElement Parent)
			: base(Parent, "XMP")
		{
		}
    }
}
