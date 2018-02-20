using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// DATALIST element
	/// </summary>
    public class DataList : HtmlElement
    {
		/// <summary>
		/// DATALIST element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public DataList(HtmlElement Parent)
			: base(Parent, "DATALIST")
		{
		}
    }
}
