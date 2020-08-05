using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class for figures with two points.
	/// </summary>
	public abstract class Point2 : Point 
	{
		private LengthAttribute x2;
		private LengthAttribute y2;
		private StringAttribute ref2;

		/// <summary>
		/// Abstract base class for figures with two points.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Point2(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.x2 = new LengthAttribute(Input, "x2");
			this.y2 = new LengthAttribute(Input, "y2");
			this.ref2 = new StringAttribute(Input, "ref2");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.x2.Export(Output);
			this.y2.Export(Output);
			this.ref2.Export(Output);
		}

	}
}
