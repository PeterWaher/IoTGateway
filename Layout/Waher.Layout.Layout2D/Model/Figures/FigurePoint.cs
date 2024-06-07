using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// Abstract base class for figures based on a point.
	/// </summary>
	public abstract class FigurePoint : Figure
	{
		private LengthAttribute x;
		private LengthAttribute y;
		private StringAttribute @ref;

		/// <summary>
		/// Abstract base class for figures based on a point.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public FigurePoint(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// X-coordinate
		/// </summary>
		public LengthAttribute XAttribute
		{
			get => this.x;
			set => this.x = value;
		}

		/// <summary>
		/// Y-coordinate
		/// </summary>
		public LengthAttribute YAttribute
		{
			get => this.y;
			set => this.y = value;
		}

		/// <summary>
		/// Reference
		/// </summary>
		public StringAttribute ReferenceAttribute
		{
			get => this.@ref;
			set => this.@ref = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.x = new LengthAttribute(Input, "x", this.Document);
			this.y = new LengthAttribute(Input, "y", this.Document);
			this.@ref = new StringAttribute(Input, "ref", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.x?.Export(Output);
			this.y?.Export(Output);
			this.@ref?.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is FigurePoint Dest)
			{
				Dest.x = this.x?.CopyIfNotPreset(Destination.Document);
				Dest.y = this.y?.CopyIfNotPreset(Destination.Document);
				Dest.@ref = this.@ref?.CopyIfNotPreset(Destination.Document);
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

			CalculatedPoint P = await this.IncludePoint(State, this.x, this.y, this.@ref, this.xCoordinate, this.yCoordinate);
			if (P.Ok)
			{
				this.xCoordinate = P.X;
				this.yCoordinate = P.Y;
			}
			else
			{
				this.xCoordinate = 0;
				this.yCoordinate = 0;

				this.IncludePoint(this.xCoordinate, this.yCoordinate);
			}
		}

		/// <summary>
		/// Measured X-coordinate
		/// </summary>
		protected float xCoordinate;

		/// <summary>
		/// Measured Y-coordinate
		/// </summary>
		protected float yCoordinate;

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.x?.ExportState(Output);
			this.y?.ExportState(Output);
			this.@ref?.ExportState(Output);
		}

	}
}
