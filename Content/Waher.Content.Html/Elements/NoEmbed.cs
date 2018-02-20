using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// NOEMBED element
	/// </summary>
    public class NoEmbed : HtmlElement
    {
		/// <summary>
		/// NOEMBED element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public NoEmbed(HtmlElement Parent)
			: base(Parent, "NOEMBED")
		{
		}
    }
}
