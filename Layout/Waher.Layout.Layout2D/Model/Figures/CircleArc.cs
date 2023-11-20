using System;
using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A circle arc
	/// </summary>
	public class CircleArc : FigurePoint
	{
		private LengthAttribute radius;
		private FloatAttribute startDegrees;
		private FloatAttribute endDegrees;
		private FloatAttribute spanDegrees;
		private BooleanAttribute clockwise;
		private BooleanAttribute center;
		private float r;
		private float start;
		private float end;
		private float span;
		private bool clockDir;
		private bool includeCenter;

		/// <summary>
		/// A circle arc
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public CircleArc(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "CircleArc";

		/// <summary>
		/// Radius
		/// </summary>
		public LengthAttribute RadiusAttribute
		{
			get => this.radius;
			set => this.radius = value;
		}

		/// <summary>
		/// Start Degrees
		/// </summary>
		public FloatAttribute StartDegreesAttribute
		{
			get => this.startDegrees;
			set => this.startDegrees = value;
		}

		/// <summary>
		/// End Degrees
		/// </summary>
		public FloatAttribute EndDegreesAttribute
		{
			get => this.endDegrees;
			set => this.endDegrees = value;
		}

		/// <summary>
		/// Span Degrees
		/// </summary>
		public FloatAttribute SpanDegreesAttribute
		{
			get => this.spanDegrees;
			set => this.spanDegrees = value;
		}

		/// <summary>
		/// Clockwise
		/// </summary>
		public BooleanAttribute ClockwiseAttribute
		{
			get => this.clockwise;
			set => this.clockwise = value;
		}

		/// <summary>
		/// Include center point
		/// </summary>
		public BooleanAttribute CenterAttribute
		{
			get => this.center;
			set => this.center = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.radius = new LengthAttribute(Input, "radius", this.Document);
			this.startDegrees = new FloatAttribute(Input, "startDegrees", this.Document);
			this.endDegrees = new FloatAttribute(Input, "endDegrees", this.Document);
			this.spanDegrees = new FloatAttribute(Input, "spanDegrees", this.Document);
			this.clockwise = new BooleanAttribute(Input, "clockwise", this.Document);
			this.center = new BooleanAttribute(Input, "center", this.Document);

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
			this.startDegrees?.Export(Output);
			this.endDegrees?.Export(Output);
			this.spanDegrees?.Export(Output);
			this.clockwise?.Export(Output);
			this.center?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new CircleArc(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is CircleArc Dest)
			{
				Dest.radius = this.radius?.CopyIfNotPreset(Destination.Document);
				Dest.startDegrees = this.startDegrees?.CopyIfNotPreset(Destination.Document);
				Dest.endDegrees = this.endDegrees?.CopyIfNotPreset(Destination.Document);
				Dest.spanDegrees = this.spanDegrees?.CopyIfNotPreset(Destination.Document);
				Dest.clockwise = this.clockwise?.CopyIfNotPreset(Destination.Document);
				Dest.center = this.center?.CopyIfNotPreset(Destination.Document);
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

			EvaluationResult<Length> RadiusLength = await this.radius.TryEvaluate(State.Session);
			if (RadiusLength.Ok)
				State.CalcDrawingSize(RadiusLength.Result, ref this.r, true);
			else
				this.defined = false;

			this.clockDir = await this.clockwise.Evaluate(State.Session, true);
			this.includeCenter = await this.center.Evaluate(State.Session, false);

			EvaluationResult<float> Degrees = await this.startDegrees.TryEvaluate(State.Session);
			if (Degrees.Ok)
				this.start = (float)Math.IEEERemainder(Degrees.Result, 360);
			else
				this.defined = false;

			Degrees = await this.endDegrees.TryEvaluate(State.Session);
			if (Degrees.Ok)
			{
				this.end = (float)Math.IEEERemainder(Degrees.Result, 360);

				if (this.clockDir)
					this.span = this.end - this.start;
				else
					this.span = this.start - this.end;

				if (this.span < 0)
					this.span += 360;
			}
			else
			{
				Degrees = await this.spanDegrees.TryEvaluate(State.Session);
				if (Degrees.Ok)
				{
					this.span = Degrees.Result;
					if (this.clockDir)
						this.end = this.start + this.span;
					else
						this.end = this.start - this.span;

					this.end = (float)Math.IEEERemainder(this.end, 360);
				}
				else
					this.defined = false;
			}

			if (this.defined)
			{
				if (this.span >= 360)
				{
					this.IncludePoint(this.xCoordinate - this.r, this.yCoordinate);
					this.IncludePoint(this.xCoordinate + this.r, this.yCoordinate);
					this.IncludePoint(this.xCoordinate, this.yCoordinate - this.r);
					this.IncludePoint(this.xCoordinate, this.yCoordinate + this.r);
				}
				else
				{
					float a = this.start;
					float r = (float)Math.IEEERemainder(this.start, 90);
					bool First = true;

					this.IncludePoint(this.xCoordinate, this.yCoordinate, this.r, this.r, this.start);

					if (this.clockDir)
					{
						if (this.end < this.start)
							this.end += 360;

						while (a < this.end)
						{
							if (First)
							{
								a += 90 - r;
								First = false;
							}
							else
								a += 90;

							this.IncludePoint(this.xCoordinate, this.yCoordinate, this.r, this.r, a);
						}
					}
					else
					{
						if (this.end > this.start)
							this.end -= 360;

						while (a > this.end)
						{
							if (First)
							{
								a -= r;
								First = false;
							}
							else
								a -= 90;

							this.IncludePoint(this.xCoordinate, this.yCoordinate, this.r, this.r, a);
						}
					}

					if (this.start != this.end)
						this.IncludePoint(this.xCoordinate, this.yCoordinate, this.r, this.r, this.end);

					if (this.includeCenter)
						this.IncludePoint(this.xCoordinate, this.yCoordinate);
				}
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
				if (this.span >= 360)
				{
					SKPaint Fill = await this.TryGetFill(State);
					if (!(Fill is null))
						State.Canvas.DrawCircle(this.xCoordinate, this.yCoordinate, this.r, Fill);

					SKPaint Pen = await this.TryGetPen(State);
					if (!(Pen is null))
						State.Canvas.DrawCircle(this.xCoordinate, this.yCoordinate, this.r, Pen);
				}
				else
				{
					float Sweep = this.clockDir ? this.span : -this.span;
					SKRect Oval = new SKRect(
						this.xCoordinate - this.r, this.yCoordinate - this.r,
						this.xCoordinate + this.r, this.yCoordinate + this.r);

					this.start = (float)Math.IEEERemainder(this.start, 360);
					if (this.start < 0)
						this.start += 360;

					this.end = (float)Math.IEEERemainder(this.end, 360);
					if (this.end < 0)
						this.end += 360;

					SKPaint Fill = await this.TryGetFill(State);
					if (!(Fill is null))
						State.Canvas.DrawArc(Oval, this.start, Sweep, this.includeCenter, Fill);

					SKPaint Pen = await this.TryGetPen(State);
					if (!(Pen is null))
						State.Canvas.DrawArc(Oval, this.start, Sweep, this.includeCenter, Pen);
				}
			}
		
			await base.Draw(State);
		}

	}
}
