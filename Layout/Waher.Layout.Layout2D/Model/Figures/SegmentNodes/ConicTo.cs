using System;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Draws a conic curve to a point, relative to the origio of the current container
	/// </summary>
	public class ConicTo : Point2Weight, ISegment, IDirectedElement
	{
		/// <summary>
		/// Draws a conic curve to a point, relative to the origio of the current container
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public ConicTo(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "ConicTo";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new ConicTo(Document, Parent);
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
				Path.ConicTo(this.P1, this.P2, this.weight);
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
			// P(t) = ((1 – t)²P₀ + 2wt(1 – t)P₁ + t²P₂) / ((1 – t)² + 2wt(1 – t) + t²)
			//      = ((1-2t+t²)P₀ + (2wt-2wt²)P₁ + t²P₂) / (1-2t+t²+2wt-2wt²+t²)
			//      = (P₀ +t(2wP₁-2P₀) + t²(P₀-2wP₁+P₂)) / (1+t(2w-2)+t²(2-2w))
			// 
			// P'(t) = ((2wP₁-2P₀+2t(P₀-2wP₁+P₂))*(1+t(2w-2)+t²(2-2w))-(P₀ +t(2wP₁-2P₀) + t²(P₀-2wP₁+P₂))*(2w-2+2t(2-2w)))/(1+t(2w-2)+t²(2-2w))²
			//
			// P'(0) = 2w(P₁-P₀)
			// P'(1) = -P₀-2wP₁+2P₂ = 2(P₂-wP₁)-P₀	Note: Error somewhere. Should be 2w(P₂-P₁) for symmetry

			float w2 = this.weight * 2;
			float dx = w2 * (P1.X - P0.X);
			float dy = w2 * (P1.Y - P0.Y);

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
			float w2 = this.weight * 2;
			float dx = w2 * (P2.X - P1.X);
			float dy = w2 * (P2.Y - P1.Y);

			X = this.P2.X;
			Y = this.P2.Y;
			Direction = CalcDirection(dx, dy);

			return this.defined;
		}
	}
}
