using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TABLE element
	/// </summary>
    public class Table : HtmlElement
    {
		/// <summary>
		/// TABLE element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Table(HtmlElement Parent)
			: base(Parent, "TABLE")
		{
		}
    }
}
