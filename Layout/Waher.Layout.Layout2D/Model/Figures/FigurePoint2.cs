using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// Abstract base class for figures with two points.
	/// </summary>
	public abstract class FigurePoint2 : FigurePoint
	{
		private LengthAttribute x2;
		private LengthAttribute y2;
		private StringAttribute ref2;

		/// <summary>
		/// Abstract base class for figures with two points.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public FigurePoint2(Layout2DDocument Document, ILayoutElement Parent)
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

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is FigurePoint2 Dest)
			{
				Dest.x2 = this.x2.CopyIfNotPreset();
				Dest.y2 = this.y2.CopyIfNotPreset();
				Dest.ref2 = this.ref2.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			if (!this.IncludePoint(State, this.x2, this.y2, this.ref2, out this.xCoordinate2, out this.yCoordinate2))
				this.defined = false;
		}

		/// <summary>
		/// Measured X-coordinate
		/// </summary>
		protected float xCoordinate2;

		/// <summary>
		/// Measured Y-coordinate
		/// </summary>
		protected float yCoordinate2;

	}
}
