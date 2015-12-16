using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.DataForms.Layout
{
	/// <summary>
	/// Class managing a text element.
	/// </summary>
	public class TextElement : LayoutElement
	{
		private string text;

		internal TextElement(XmlElement E)
		{
			this.text = E.InnerText;
		}

		/// <summary>
		/// Text
		/// </summary>
		public string Text { get { return this.text; } }

	}
}
