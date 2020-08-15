using System;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Draws a line to a point that lies a certain distance backward of the last point,
	/// in the current direction of movement.
	/// </summary>
	public class Backward : Distance, ISegment, IDirectedElement
	{
		/// <summary>
		/// Draws a line to a point that lies a certain distance backward of the last point,
		/// in the current direction of movement.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Backward(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Backward";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Backward(Document, Parent);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public void Measure(DrawingState State, PathState PathState)
		{
			if (this.defined)
				PathState.Backward(this.dist);
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
				this.P1 = Path.LastPoint;
				Path.LineTo(this.P2 = PathState.Backward(this.dist));
			}
		}

		/// <summary>
		/// Line drawn from this point
		/// </summary>
		protected SKPoint P1;

		/// <summary>
		/// Line drawn to this point
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
			X = this.P1.X;
			Y = this.P1.Y;
			Direction = CalcDirection(this.P1, this.P2);

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
			X = this.P2.X;
			Y = this.P2.Y;
			Direction = CalcDirection(this.P1, this.P2);

			return this.defined;
		}

	}
}
