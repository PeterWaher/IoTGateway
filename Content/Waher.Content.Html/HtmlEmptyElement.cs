using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html
{
	/// <summary>
	/// Base class for all empty HTML elements.
	/// </summary>
	public class HtmlEmptyElement : HtmlElement
    {
		/// <summary>
		/// Base class for all empty HTML elements.
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="Name">Tag name.</param>
		public HtmlEmptyElement(HtmlDocument Document, HtmlElement Parent, int StartPosition, string Name)
			: base(Document, Parent, StartPosition, Name)
		{
		}

		/// <summary>
		/// If the element is an empty element.
		/// </summary>
		public override bool IsEmptyElement
		{
			get { return true; }
		}
	}
}
