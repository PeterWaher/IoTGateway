using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content;

namespace Waher.Networking.XMPP.DataForms.Layout
{
	/// <summary>
	/// Class managing a section within a page in a data form layout.
	/// </summary>
	public class Section : LayoutElement
	{
		private string label;
		private LayoutElement[] elements;

		internal Section(XmlElement E)
		{
			List<LayoutElement> Elements = new List<LayoutElement>();

			this.label = XML.Attribute(E, "label");

			foreach (XmlNode N in E.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "text":
						Elements.Add(new TextElement((XmlElement)N));
						break;

					case "section":
						Elements.Add(new Section((XmlElement)N));
						break;

					case "fieldref":
						Elements.Add(new FieldReference((XmlElement)N));
						break;

					case "reportedref":
						Elements.Add(new ReportedReference());
						break;
				}
			}

			this.elements = Elements.ToArray();
		}

		internal Section(string Title, Field[] Fields)
		{
			this.label = Title;

			int i, c = Fields.Length;
			this.elements = new LayoutElement[c];

			for (i = 0; i < c; i++)
				this.elements[i] = new FieldReference(Fields[i].Var);
		}

		/// <summary>
		/// Label
		/// </summary>
		public string Label { get { return this.label; } }

		/// <summary>
		/// Embedded layout elements.
		/// </summary>
		public LayoutElement[] Elements { get { return this.elements; } }

		/// <summary>
		/// Exports the form to XAML.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Form">Data form containing element.</param>
		public override void ExportXAML(XmlWriter Output, DataForm Form)
		{
			Output.WriteStartElement("GroupBox");
			Output.WriteAttributeString("Header", this.label);
			// TODO: Margins, Padding, Color, Thickness, etc.
			
			Output.WriteStartElement("StackPanel");

			foreach (LayoutElement Element in this.elements)
				Element.ExportXAML(Output, Form);

			Output.WriteEndElement();
			Output.WriteEndElement();
		}
	}
}
