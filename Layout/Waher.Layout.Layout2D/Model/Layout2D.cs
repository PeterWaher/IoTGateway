using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Backgrounds
{
	/// <summary>
	/// Root node for two-dimensional layouts
	/// </summary>
	public class Layout2D : LayoutContainer
	{
		private StringAttribute fontId;
		private StringAttribute penId;
		private StringAttribute backgroundId;
		private ColorAttribute textColor;

		/// <summary>
		/// Root node for two-dimensional layouts
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Layout2D(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Layout2D";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.fontId = new StringAttribute(Input, "fontId");
			this.penId = new StringAttribute(Input, "penId");
			this.backgroundId = new StringAttribute(Input, "backgroundId");
			this.textColor = new ColorAttribute(Input, "textColor");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.fontId.Export(Output);
			this.penId.Export(Output);
			this.backgroundId.Export(Output);
			this.textColor.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Layout2D(Document, Parent);
		}
	}
}
