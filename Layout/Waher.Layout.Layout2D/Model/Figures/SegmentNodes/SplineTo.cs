using System;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Draws a spline to a point, relative to the origio of the current container
	/// </summary>
	public class SplineTo : Point, ISegment, IDirectedElement
	{
		/// <summary>
		/// Draws a spline to a point, relative to the origio of the current container
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public SplineTo(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "SplineTo";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new SplineTo(Document, Parent);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public virtual void Measure(DrawingState State, PathState PathState)
		{
			if (this.defined)
				PathState.SetSplineVertex(this.xCoordinate, this.yCoordinate);
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		/// <param name="Path">Path being generated.</param>
		public virtual void Draw(DrawingState State, PathState PathState, SKPath Path)
		{
			if (this.defined)
				this.splineCurve = PathState.SetSplineVertex(this.xCoordinate, this.yCoordinate);
		}

		/// <summary>
		/// Dynamic spline curve.
		/// </summary>
		protected PathSpline splineCurve;

		/// <summary>
		/// Tries to get start position and initial direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Initial direction.</param>
		/// <returns>If a start position was found.</returns>
		public bool TryGetStart(out float X, out float Y, out float Direction)
		{
			if (this.splineCurve is null)
			{
				X = Y = Direction = 0;
				return false;
			}
			else
				return this.splineCurve.TryGetStart(out X, out Y, out Direction);
		}

		/// <summary>
		/// Tries to get end position and terminating direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Terminating direction.</param>
		/// <returns>If a terminating position was found.</returns>
		public bool TryGetEnd(out float X, out float Y, out float Direction)
		{
			if (this.splineCurve is null)
			{
				X = Y = Direction = 0;
				return false;
			}
			else
				return this.splineCurve.TryGetEnd(out X, out Y, out Direction);
		}

	}
}
