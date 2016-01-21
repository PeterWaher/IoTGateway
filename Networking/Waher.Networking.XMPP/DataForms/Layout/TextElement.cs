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

		/// <summary>
		/// Exports the form to XAML.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Form">Data form containing element.</param>
		public override void ExportXAML(XmlWriter Output, DataForm Form)
		{
			Output.WriteStartElement("TextBlock");
			Output.WriteAttributeString("TextWrapping", "Wrap");
			// TODO: Margins

			Output.WriteValue(this.text);
			Output.WriteEndElement();
		}

	}
}
