using System;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Draws a cubic curve to a point, relative to the origio of the current container
	/// </summary>
	public class CubicTo : Point3, ISegment, IDirectedElement
	{
		/// <summary>
		/// Draws a cubic curve to a point, relative to the origio of the current container
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public CubicTo(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "CubicTo";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new CubicTo(Document, Parent);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public virtual void Measure(DrawingState State, PathState PathState)
		{
			if (this.defined)
			{
				PathState.Set(this.xCoordinate, this.yCoordinate);
				PathState.Set(this.xCoordinate2, this.yCoordinate2);
				PathState.Set(this.xCoordinate3, this.yCoordinate3);
			}
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
			{
				this.P0 = Path.LastPoint;
				this.P1 = new SKPoint(this.xCoordinate, this.yCoordinate);
				this.P2 = new SKPoint(this.xCoordinate2, this.yCoordinate2);
				this.P3 = new SKPoint(this.xCoordinate3, this.yCoordinate3);

				PathState.Set(this.xCoordinate, this.yCoordinate);
				PathState.Set(this.xCoordinate2, this.yCoordinate2);
				PathState.Set(this.xCoordinate3, this.yCoordinate3);
				Path.CubicTo(this.P1, this.P2, this.P3);
			}
		}

		/// <summary>
		/// Starting point.
		/// </summary>
		protected SKPoint P0;

		/// <summary>
		/// Intermediate point 1.
		/// </summary>
		protected SKPoint P1;

		/// <summary>
		/// Intermediate point 2.
		/// </summary>
		protected SKPoint P2;

		/// <summary>
		/// Ending point.
		/// </summary>
		protected SKPoint P3;

		/// <summary>
		/// Tries to get start position and initial direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Initial direction.</param>
		/// <returns>If a start position was found.</returns>
		public bool TryGetStart(out float X, out float Y, out float Direction)
		{
			// B(t) = (1-t)³P₀+3(1-t)²tP₁+3(1-t)t²P₂+t³P₃
			// B'(t) = 3(1-t)²(P₁-P₀)+6(1-t)t(P₂-P₁)+3t²(P₃-P₂)
			//
			// B'(0) = 3(P₁-P₀)
			// B'(1) = 3(P₃-P₂)
			//
			// Ratio will be compared, so no need to multiply with 3.

			float dx = P1.X - P0.X;
			float dy = P1.Y - P0.Y;

			X = this.P0.X;
			Y = this.P0.Y;
			Direction = CalcDirection(dx, dy);

			return this.defined;
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
			float dx = P3.X - P2.X;
			float dy = P3.Y - P2.Y;

			X = this.P3.X;
			Y = this.P3.Y;
			Direction = CalcDirection(dx, dy);

			return this.defined;
		}

	}
}
