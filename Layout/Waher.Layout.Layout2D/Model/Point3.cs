using System.Threading.Tasks;
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
		public override Task FromXml(XmlElement Input)
		{
			this.x3 = new LengthAttribute(Input, "x3", this.Document);
			this.y3 = new LengthAttribute(Input, "y3", this.Document);
			this.ref3 = new StringAttribute(Input, "ref3", this.Document);

			return base.FromXml(Input);
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
				Dest.x3 = this.x3?.CopyIfNotPreset(Destination.Document);
				Dest.y3 = this.y3?.CopyIfNotPreset(Destination.Document);
				Dest.ref3 = this.ref3?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			await base.DoMeasureDimensions(State);

			CalculatedPoint P = await this.CalcPoint(State, this.x3, this.y3, this.@ref3, this.xCoordinate3, this.yCoordinate3);
			if (P.Ok)
			{
				this.xCoordinate3 = P.X;
				this.yCoordinate3 = P.Y;
			}
			else
				this.defined = false;
		}

		/// <summary>
		/// Measured X-coordinate
		/// </summary>
		protected float xCoordinate3;

		/// <summary>
		/// Measured Y-coordinate
		/// </summary>
		protected float yCoordinate3;

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.x3?.ExportState(Output);
			this.y3?.ExportState(Output);
			this.ref3?.ExportState(Output);
		}

	}
}
