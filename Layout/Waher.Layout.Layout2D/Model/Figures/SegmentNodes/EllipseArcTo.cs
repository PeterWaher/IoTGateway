﻿using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Draws a ellipse arc to a point, relative to the origio of the current container
	/// </summary>
	public class EllipseArcTo : Point, ISegment
	{
		private LengthAttribute radiusX;
		private LengthAttribute radiusY;
		private BooleanAttribute clockwise;

		/// <summary>
		/// Draws a ellipse arc to a point, relative to the origio of the current container
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public EllipseArcTo(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "EllipseArcTo";

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
		/// Clockwise
		/// </summary>
		public BooleanAttribute ClockwiseAttribute
		{
			get => this.clockwise;
			set => this.clockwise = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.radiusX = new LengthAttribute(Input, "radiusX", this.Document);
			this.radiusY = new LengthAttribute(Input, "radiusY", this.Document);
			this.clockwise = new BooleanAttribute(Input, "clockwise", this.Document);

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
			this.clockwise?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new EllipseArcTo(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is EllipseArcTo Dest)
			{
				Dest.radiusX = this.radiusX?.CopyIfNotPreset(Destination.Document);
				Dest.radiusY = this.radiusY?.CopyIfNotPreset(Destination.Document);
				Dest.clockwise = this.clockwise?.CopyIfNotPreset(Destination.Document);
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
				State.CalcDrawingSize(RadiusLength.Result, ref this.rX, true, this);
			else
				this.defined = false;

			RadiusLength = await this.radiusY.TryEvaluate(State.Session);
			if (RadiusLength.Ok)
				State.CalcDrawingSize(RadiusLength.Result, ref this.rY, false, this);
			else
				this.defined = false;

			EvaluationResult<bool> Clockwise = await this.clockwise.TryEvaluate(State.Session);
			if (Clockwise.Ok)
				this.clockDir = Clockwise.Result;
			else
				this.defined = false;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public virtual Task Measure(DrawingState State, PathState PathState)
		{
			if (this.defined)
				PathState.Set(this.xCoordinate, this.yCoordinate);
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Measured radius along X-axis
		/// </summary>
		protected float rX;

		/// <summary>
		/// Measured radius along Y-axis
		/// </summary>
		protected float rY;

		/// <summary>
		/// Measured direction of arc
		/// </summary>
		protected bool clockDir;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		/// <param name="Path">Path being generated.</param>
		public virtual Task Draw(DrawingState State, PathState PathState, SKPath Path)
		{
			if (this.defined)
			{
				PathState.Set(this.xCoordinate, this.yCoordinate);
				Path.ArcTo(this.rX, this.rY, 0, SKPathArcSize.Small,
					this.clockDir ? SKPathDirection.Clockwise : SKPathDirection.CounterClockwise,
					this.xCoordinate, this.yCoordinate);
			}
	
			return Task.CompletedTask;
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
			this.clockwise?.ExportState(Output);
		}

		// TODO: IDirectedElement
	}
}
