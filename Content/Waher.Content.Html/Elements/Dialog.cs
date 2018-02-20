using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DIALOG element
	/// </summary>
    public class Dialog : HtmlElement
    {
		/// <summary>
		/// DIALOG element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Dialog(HtmlElement Parent)
			: base(Parent, "DIALOG")
		{
		}
    }
}
