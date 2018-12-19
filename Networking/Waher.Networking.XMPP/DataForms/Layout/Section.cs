using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

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
		private int priority = 0;
		private int ordinal = 0;

		/// <summary>
		/// Class managing a section within a page in a data form layout.
		/// </summary>
		/// <param name="Form">Data Form.</param>
		/// <param name="Label">Label</param>
		/// <param name="ChildElements">Child elements.</param>
		public Section(DataForm Form, string Label, params LayoutElement[] ChildElements)
			: base(Form)
		{
			this.label = Label;
			this.staticElements = ChildElements;
		}

		/// <summary>
		/// Class managing a section within a page in a data form layout.
		/// </summary>
		/// <param name="Form">Data Form.</param>
		/// <param name="Label">Label</param>
		/// <param name="Fields">Fields to include in section. These will be converted to <see cref="FieldReference"/> objects.</param>
		public Section(DataForm Form, string Label, Field[] Fields)
			: base(Form)
		{
			this.label = Label;

			int i, c = Fields.Length;
			this.staticElements = new LayoutElement[c];

			for (i = 0; i < c; i++)
				this.staticElements[i] = new FieldReference(Form, Fields[i].Var);
		}

		internal Section(DataForm Form, XmlElement E)
			: base(Form)
		{
			List<LayoutElement> Elements = new List<LayoutElement>();

			this.label = XML.Attribute(E, "label");

			foreach (XmlNode N in E.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "text":
						Elements.Add(new TextElement(this.Form, (XmlElement)N));
						break;

					case "section":
						Elements.Add(new Section(this.Form, (XmlElement)N));
						break;

					case "fieldref":
						Elements.Add(new FieldReference(this.Form, (XmlElement)N));
						break;

					case "reportedref":
						Elements.Add(new ReportedReference(this.Form));
						break;
				}
			}

			this.staticElements = Elements.ToArray();
		}

		internal Section(DataForm Form, string Title, ReportedReference ReportedReference)
			: base(Form)
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
		/// Can be used to sort pages or sections. Not serialized to or from XML.
		/// </summary>
		public int Priority
		{
			get { return this.priority; }
			set { this.priority = value; }
		}

		/// <summary>
		/// Can be used to sort pages or sections having the same priority. Not serialized to or from XML.
		/// </summary>
		public int Ordinal
		{
			get { return this.ordinal; }
			set { this.ordinal = value; }
		}

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
			if (this.dynamicElements is null)
			{
				this.dynamicElements = new List<LayoutElement>();
				this.dynamicElements.AddRange(this.staticElements);
			}

			this.dynamicElements.Add(Element);
		}

		internal override bool RemoveExcluded()
		{
			if (this.dynamicElements is null)
			{
				this.dynamicElements = new List<LayoutElement>();
				this.dynamicElements.AddRange(this.staticElements);
			}

			int i = 0;
			int c = this.dynamicElements.Count;
			LayoutElement E;

			while (i < c)
			{
				E = this.dynamicElements[i];

				if (E.RemoveExcluded())
				{
					this.dynamicElements.RemoveAt(i);
					c--;
				}
				else
					i++;
			}

			return c == 0;
		}

		internal override void Serialize(StringBuilder Output)
		{
			Output.Append("<xdl:section label='");
			Output.Append(XML.Encode(this.Label));
			Output.Append("'>");

			foreach (LayoutElement E in this.Elements)
				E.Serialize(Output);

			Output.Append("</xdl:section>");
		}

		/// <summary>
		/// Sorts the contents of the layout element.
		/// </summary>
		public override void Sort()
		{
			if (this.dynamicElements != null)
			{
				this.dynamicElements.Sort(this.OrderElements);

				foreach (LayoutElement E in this.dynamicElements)
					E.Sort();
			}
			else
			{
				Array.Sort<LayoutElement>(this.staticElements, this.OrderElements);

				foreach (LayoutElement E in this.staticElements)
					E.Sort();
			}
		}

		private int OrderElements(LayoutElement x, LayoutElement y)
		{
			int i;

			if (x is FieldReference frx && y is FieldReference fry)
			{
				Field fx = this.Form[frx.Var];
				Field fy = this.Form[fry.Var];

				if (fx is null || fy is null)
					return 0;

				i = fx.Priority - fy.Priority;
				if (i != 0)
					return i;

				return fx.Ordinal - fy.Ordinal;
			}
			else if (x is Section sx && y is Section sy)
			{
				i = sx.Priority - sy.Priority;
				if (i != 0)
					return i;

				return sx.Ordinal - sy.Ordinal;
			}
			else
				return 0;
		}

	}
}
