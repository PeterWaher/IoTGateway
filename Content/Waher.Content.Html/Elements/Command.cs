using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// COMMAND element
	/// </summary>
    public class Command : HtmlElement
    {
		/// <summary>
		/// COMMAND element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Command(HtmlElement Parent)
			: base(Parent, "COMMAND")
		{
		}
    }
}
