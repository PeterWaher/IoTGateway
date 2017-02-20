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
		private LayoutElement[] staticElements;
		private List<LayoutElement> dynamicElements = null;

		/// <summary>
		/// Class managing a section within a page in a data form layout.
		/// </summary>
		/// <param name="Label">Label</param>
		/// <param name="ChildElements">Child elements.</param>
		public Section(string Label, params LayoutElement[] ChildElements)
		{
			this.label = Label;
			this.staticElements = ChildElements;
		}

		/// <summary>
		/// Class managing a section within a page in a data form layout.
		/// </summary>
		/// <param name="Label">Label</param>
		/// <param name="Fields">Fields to include in section. These will be converted to <see cref="FieldReference"/> objects.</param>
		public Section(string Label, Field[] Fields)
		{
			this.label = Label;

			int i, c = Fields.Length;
			this.staticElements = new LayoutElement[c];

			for (i = 0; i < c; i++)
				this.staticElements[i] = new FieldReference(Fields[i].Var);
		}

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

			this.staticElements = Elements.ToArray();
		}

		internal Section(string Title, ReportedReference ReportedReference)
		{
			this.label = Title;
			this.staticElements = new LayoutElement[1];
			this.staticElements[0] = ReportedReference;
		}

		/// <summary>
		/// Label
		/// </summary>
		public string Label { get { return this.label; } }

		/// <summary>
		/// Embedded layout elements.
		/// </summary>
		public LayoutElement[] Elements
		{
			get
			{
				if (this.dynamicElements != null)
				{
					this.staticElements = this.dynamicElements.ToArray();
					this.dynamicElements = null;
				}

				return this.staticElements;
			}
		}

		/// <summary>
		/// Adds a layout element.
		/// </summary>
		/// <param name="Element">Layout element.</param>
		public void Add(LayoutElement Element)
		{
			if (this.dynamicElements == null)
			{
				this.dynamicElements = new List<LayoutElement>();
				this.dynamicElements.AddRange(this.staticElements);
			}

			this.dynamicElements.Add(Element);
		}

	}
}
