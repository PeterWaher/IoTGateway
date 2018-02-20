using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// H&lt;n&gt; element
	/// </summary>
    public class Hn : HtmlElement
    {
		/// <summary>
		/// H&lt;n&gt; element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Hn(HtmlElement Parent, int Level)
			: base(Parent, "H" + Level.ToString())
		{
		}
    }
}
