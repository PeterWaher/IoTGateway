using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// TRACK element
	/// </summary>
    public class Track : HtmlEmptyElement
	{
		/// <summary>
		/// TRACK element
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		public Track(HtmlElement Parent)
			: base(Parent, "TRACK")
		{
		}
    }
}
