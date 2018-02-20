using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DATA element
	/// </summary>
    public class Data : HtmlElement
    {
		/// <summary>
		/// DATA element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Data(HtmlElement Parent)
			: base(Parent, "DATA")
		{
		}
    }
}
