using System;
using SkiaSharp;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Moves to a new coordinate.
	/// </summary>
	public class MoveTo : OneCoordinate
	{
		/// <summary>
		/// Moves to a new coordinate.
		/// </summary>
		public MoveTo()
			: base()
		{
		}

		/// <summary>
		/// Moves to a new coordinate.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		public MoveTo(float X, float Y)
			: base(X, Y)
		{
		}

		/// <inheritdoc/>
		public override void Draw(SKCanvas Canvas, CanvasState State)
		{
			State.X = this.X;
			State.Y = this.Y;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is MoveTo Obj && base.Equals(Obj));
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
