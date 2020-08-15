using System;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Draws a quadratic curve to a point, relative to the origio of the current container
	/// </summary>
	public class QuadraticTo : Point2, ISegment, IDirectedElement
	{
		/// <summary>
		/// Draws a quadratic curve to a point, relative to the origio of the current container
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public QuadraticTo(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "QuadraticTo";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new QuadraticTo(Document, Parent);
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

				PathState.Set(this.xCoordinate, this.yCoordinate);
				PathState.Set(this.xCoordinate2, this.yCoordinate2);
				Path.QuadTo(this.P1, this.P2);
			}
		}

		/// <summary>
		/// Starting point.
		/// </summary>
		protected SKPoint P0;

		/// <summary>
		/// Intermediate point.
		/// </summary>
		protected SKPoint P1;

		/// <summary>
		/// Ending point.
		/// </summary>
		protected SKPoint P2;

		/// <summary>
		/// Tries to get start position and initial direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Initial direction.</param>
		/// <returns>If a start position was found.</returns>
		public bool TryGetStart(out float X, out float Y, out float Direction)
		{
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
			float dx = P2.X - P1.X;
			float dy = P2.Y - P1.Y;

			X = this.P2.X;
			Y = this.P2.Y;
			Direction = CalcDirection(dx, dy);

			return this.defined;
		}

	}
}
