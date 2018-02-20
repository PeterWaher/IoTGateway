using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// RT element
	/// </summary>
    public class Rt : HtmlElement
    {
		/// <summary>
		/// RT element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Rt(HtmlElement Parent)
			: base(Parent, "RT")
		{
		}
    }
}
