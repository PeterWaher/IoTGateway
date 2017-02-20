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

		/// <summary>
		/// Class managing a text element.
		/// </summary>
		/// <param name="Form">Data Form.</param>
		/// <param name="Text">Text.</param>
		public TextElement(DataForm Form, string Text)
			: base(Form)
		{
			this.text = Text;
		}

		internal TextElement(DataForm Form, XmlElement E)
			: base(Form)
		{
			this.text = E.InnerText;
		}

		/// <summary>
		/// Text
		/// </summary>
		public string Text { get { return this.text; } }

		internal override bool RemoveExcluded()
		{
			return false;
		}
	}
}
