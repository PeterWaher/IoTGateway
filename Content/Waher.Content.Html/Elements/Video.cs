using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// VIDEO element
	/// </summary>
    public class Video : HtmlElement
    {
		/// <summary>
		/// VIDEO element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Video(HtmlElement Parent)
			: base(Parent, "VIDEO")
		{
		}
    }
}
