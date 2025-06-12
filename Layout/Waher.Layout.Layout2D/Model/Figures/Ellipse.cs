using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// An ellipse
	/// </summary>
	public class Ellipse : FigurePoint
	{
		private LengthAttribute radiusX;
		private LengthAttribute radiusY;
		private float rX;
		private float rY;

		/// <summary>
		/// An ellipse
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Ellipse(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Ellipse";

		/// <summary>
		/// Radius X
		/// </summary>
		public LengthAttribute RadiusXAttribute
		{
			get => this.radiusX;
			set => this.radiusX = value;
		}

		/// <summary>
		/// Radius Y
		/// </summary>
		public LengthAttribute RadiusYAttribute
		{
			get => this.radiusY;
			set => this.radiusY = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.radiusX = new LengthAttribute(Input, "radiusX", this.Document);
			this.radiusY = new LengthAttribute(Input, "radiusY", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.radiusX?.Export(Output);
			this.radiusY?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Ellipse(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Ellipse Dest)
			{
				Dest.radiusX = this.radiusX?.CopyIfNotPreset(Destination.Document);
				Dest.radiusY = this.radiusY?.CopyIfNotPreset(Destination.Document);
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

			EvaluationResult<Length> RadiusLength = await this.radiusX.TryEvaluate(State.Session);
			if (RadiusLength.Ok)
			{
				State.CalcDrawingSize(RadiusLength.Result, ref this.rX, true, (ILayoutElement)this);
				this.Width = this.ExplicitWidth = 2 * this.rX;
			}
			else
				this.defined = false;

			RadiusLength = await this.radiusY.TryEvaluate(State.Session);
			if (RadiusLength.Ok)
			{
				State.CalcDrawingSize(RadiusLength.Result, ref this.rY, false, (ILayoutElement)this);
				this.Height = this.ExplicitHeight = 2 * this.rY;
			}
			else
				this.defined = false;

			if (this.defined)
			{
				this.IncludePoint(this.xCoordinate - this.rX, this.yCoordinate);
				this.IncludePoint(this.xCoordinate + this.rX, this.yCoordinate);
				this.IncludePoint(this.xCoordinate, this.yCoordinate - this.rY);
				this.IncludePoint(this.xCoordinate, this.yCoordinate + this.rY);
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
					State.Canvas.DrawOval(this.xCoordinate, this.yCoordinate, this.rX, this.rY, Fill);

				SKPaint Pen = await this.TryGetPen(State);
				if (!(Pen is null))
					State.Canvas.DrawOval(this.xCoordinate, this.yCoordinate, this.rX, this.rY, Pen);
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

			this.radiusX?.ExportState(Output);
			this.radiusY?.ExportState(Output);
		}

	}
}
