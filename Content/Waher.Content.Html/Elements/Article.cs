using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// ARTICLE element
	/// </summary>
    public class Article : HtmlElement
    {
		/// <summary>
		/// ARTICLE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Article(HtmlElement Parent)
			: base(Parent, "ARTICLE")
		{
		}
    }
}
