using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// PLAINTEXT element
	/// </summary>
    public class PlainText : HtmlElement
    {
		/// <summary>
		/// PLAINTEXT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public PlainText(HtmlElement Parent)
			: base(Parent, "PLAINTEXT")
		{
		}
    }
}
