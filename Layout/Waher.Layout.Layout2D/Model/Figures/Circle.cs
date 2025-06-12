using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A circle
	/// </summary>
	public class Circle : FigurePoint
	{
		private LengthAttribute radius;
		private float r;

		/// <summary>
		/// A circle
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Circle(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Circle";

		/// <summary>
		/// Radius
		/// </summary>
		public LengthAttribute RadiusAttribute
		{
			get => this.radius;
			set => this.radius = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.radius = new LengthAttribute(Input, "radius", this.Document);
			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.radius?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Circle(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Circle Dest)
				Dest.radius = this.radius?.CopyIfNotPreset(Destination.Document);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			await base.DoMeasureDimensions(State);

			EvaluationResult<Length> RadiusLength = await this.radius.TryEvaluate(State.Session);
			if (RadiusLength.Ok)
			{
				State.CalcDrawingSize(RadiusLength.Result, ref this.r, true, (ILayoutElement)this);
				this.Width = this.ExplicitWidth = this.Height = this.ExplicitHeight = 2 * this.r;
			}
			else
				this.defined = false;

			if (this.defined)
			{
				this.IncludePoint(this.xCoordinate - this.r, this.yCoordinate);
				this.IncludePoint(this.xCoordinate + this.r, this.yCoordinate);
				this.IncludePoint(this.xCoordinate, this.yCoordinate - this.r);
				this.IncludePoint(this.xCoordinate, this.yCoordinate + this.r);
			}
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			if (this.defined)
			{
				SKPaint Fill = await this.TryGetFill(State);
				if (!(Fill is null))
					State.Canvas.DrawCircle(this.xCoordinate, this.yCoordinate, this.r, Fill);

				SKPaint Pen = await this.TryGetPen(State);
				if (!(Pen is null))
					State.Canvas.DrawCircle(this.xCoordinate, this.yCoordinate, this.r, Pen);
			}
		
			await base.Draw(State);
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.radius?.ExportState(Output);
		}
	}
}
