using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// SUMMARY element
	/// </summary>
    public class Summary : HtmlElement
    {
		/// <summary>
		/// SUMMARY element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Summary(HtmlElement Parent)
			: base(Parent, "SUMMARY")
		{
		}
    }
}
