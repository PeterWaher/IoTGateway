using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Filters
{
	/// <summary>
	/// Shadow filter
	/// </summary>
	public class Shadow : LayoutContainer
	{
		private LengthAttribute dX;
		private LengthAttribute dY;
		private DoubleAttribute sigmaX;
		private DoubleAttribute sigmaY;
		private ColorAttribute color;

		/// <summary>
		/// Shadow filter
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Shadow(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Shadow";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.dX = new LengthAttribute(Input, "dX");
			this.dY = new LengthAttribute(Input, "dY");
			this.sigmaX = new DoubleAttribute(Input, "sigmaX");
			this.sigmaY = new DoubleAttribute(Input, "sigmaY");
			this.color = new ColorAttribute(Input, "color");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.dX.Export(Output);
			this.dY.Export(Output);
			this.sigmaX.Export(Output);
			this.sigmaY.Export(Output);
			this.color.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Shadow(Document, Parent);
		}
	}
}
