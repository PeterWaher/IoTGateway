using System;
using SkiaSharp;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Sets the current pen width.
	/// </summary>
	public class PenWidth : OneFloatParameter
	{
		/// <summary>
		/// Sets the current pen width.
		/// </summary>
		public PenWidth()
			: base()
		{
		}

		/// <summary>
		/// Sets the current pen width.
		/// </summary>
		/// <param name="Width">New pen width</param>
		public PenWidth(float Width)
			: base(Width)
		{
		}

		/// <inheritdoc/>
		public override void Draw(SKCanvas Canvas, CanvasState State)
		{
			State.Width = this.Parameter;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is PenWidth Obj && base.Equals(Obj));
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
