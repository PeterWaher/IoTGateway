using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// PROGRESS element
	/// </summary>
    public class Progress : HtmlElement
    {
		/// <summary>
		/// PROGRESS element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Progress(HtmlElement Parent)
			: base(Parent, "PROGRESS")
		{
		}
    }
}
