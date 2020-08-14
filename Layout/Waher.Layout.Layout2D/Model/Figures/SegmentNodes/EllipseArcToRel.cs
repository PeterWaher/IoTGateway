using System;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Draws a ellipse arc to a point, relative to the end of the last segment
	/// </summary>
	public class EllipseArcToRel : EllipseArcTo
	{
		/// <summary>
		/// Draws a ellipse arc to a point, relative to the end of the last segment
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public EllipseArcToRel(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "EllipseArcToRel";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new EllipseArcToRel(Document, Parent);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public override void Measure(DrawingState State, PathState PathState)
		{
			if (this.defined)
				PathState.Add(this.xCoordinate, this.yCoordinate);
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		/// <param name="Path">Path being generated.</param>
		public override void Draw(DrawingState State, PathState PathState, SKPath Path)
		{
			if (this.defined)
			{
				SKPoint P = PathState.Add(this.xCoordinate, this.yCoordinate);
				Path.ArcTo(this.rX, this.rY, 0, SKPathArcSize.Small,
					this.clockDir ? SKPathDirection.Clockwise : SKPathDirection.CounterClockwise,
					P.X, P.Y);
			}
		}
	}
}
