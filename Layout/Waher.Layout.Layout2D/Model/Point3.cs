using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class for figures with three points.
	/// </summary>
	public abstract class Point3 : Point2
	{
		private LengthAttribute x3;
		private LengthAttribute y3;
		private StringAttribute ref3;

		/// <summary>
		/// Abstract base class for figures with three points.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Point3(Layout2DDocument Document, ILayoutElement Parent)
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

			this.x3 = new LengthAttribute(Input, "x3");
			this.y3 = new LengthAttribute(Input, "y3");
			this.ref3 = new StringAttribute(Input, "ref3");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.x3.Export(Output);
			this.y3.Export(Output);
			this.ref3.Export(Output);
		}

	}
}
