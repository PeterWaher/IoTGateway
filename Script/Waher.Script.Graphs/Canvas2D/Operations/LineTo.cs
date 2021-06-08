using System;
using SkiaSharp;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Draws a line to a specific coordinate
	/// </summary>
	public class LineTo : OneCoordinate
	{
		/// <summary>
		/// Draws a line to a specific coordinate
		/// </summary>
		public LineTo()
			: base()
		{
		}

		/// <summary>
		/// Draws a line to a specific coordinate
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		public LineTo(float X, float Y)
			: base(X, Y)
		{
		}

		/// <inheritdoc/>
		public override void Draw(SKCanvas Canvas, CanvasState State)
		{
			Canvas.DrawLine(State.X, State.Y, this.X, this.Y, State.Pen);
			State.X = this.X;
			State.Y = this.Y;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is LineTo Obj && base.Equals(Obj));
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
