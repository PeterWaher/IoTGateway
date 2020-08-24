using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class for layout elements with three points.
	/// </summary>
	public abstract class Point3 : Point2
	{
		private LengthAttribute x3;
		private LengthAttribute y3;
		private StringAttribute ref3;

		/// <summary>
		/// Abstract base class for layout elements with three points.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Point3(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// X-coordinate 3
		/// </summary>
		public LengthAttribute X3Attribute
		{
			get => this.x3;
			set => this.x3 = value;
		}

		/// <summary>
		/// Y-coordinate 3
		/// </summary>
		public LengthAttribute Y3Attribute
		{
			get => this.y3;
			set => this.y3 = value;
		}

		/// <summary>
		/// Reference 3
		/// </summary>
		public StringAttribute Reference3Attribute
		{
			get => this.ref3;
			set => this.ref3 = value;
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

			this.x3?.Export(Output);
			this.y3?.Export(Output);
			this.ref3?.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Point3 Dest)
			{
				Dest.x3 = this.x3?.CopyIfNotPreset();
				Dest.y3 = this.y3?.CopyIfNotPreset();
				Dest.ref3 = this.ref3?.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool DoMeasureDimensions(DrawingState State)
		{
			bool Relative = base.DoMeasureDimensions(State);

			if (!this.CalcPoint(State, this.x3, this.y3, this.ref3, ref this.xCoordinate3, ref this.yCoordinate3, ref Relative))
				this.defined = false;

			return Relative;
		}

		/// <summary>
		/// Measured X-coordinate
		/// </summary>
		protected float xCoordinate3;

		/// <summary>
		/// Measured Y-coordinate
		/// </summary>
		protected float yCoordinate3;

	}
}
