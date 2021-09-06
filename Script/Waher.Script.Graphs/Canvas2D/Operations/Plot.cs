using System;
using SkiaSharp;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Plots a pixel at a given coordinate.
	/// </summary>
	public class Plot : OneCoordinate
	{
		/// <summary>
		/// Plots a pixel at a given coordinate.
		/// </summary>
		public Plot()
			: base()
		{
		}

		/// <summary>
		/// Plots a pixel at a given coordinate.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		public Plot(float X, float Y)
			: base(X, Y)
		{
		}

		/// <inheritdoc/>
		public override void Draw(SKCanvas Canvas, CanvasState State)
		{
			Canvas.DrawPoint(this.X, this.Y, State.Pen);
			State.X = this.X;
			State.Y = this.Y;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is Plot Obj && base.Equals(Obj));
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.GetType().FullName.GetHashCode();
			Result ^= Result << 5 ^ base.GetHashCode();
			return Result;
		}
	}
}
