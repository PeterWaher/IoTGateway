using System;
using SkiaSharp;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Sets the current pen color.
	/// </summary>
	public class PenColor : OneColorParameter
	{
		/// <summary>
		/// Sets the current pen color.
		/// </summary>
		public PenColor()
			: base()
		{
		}

		/// <summary>
		/// Sets the current pen color.
		/// </summary>
		/// <param name="Color">New color.</param>
		public PenColor(SKColor Color)
			: base(Color)
		{
		}

		/// <inheritdoc/>
		public override void Draw(SKCanvas Canvas, CanvasState State)
		{
			State.FgColor = this.Parameter;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is PenColor Obj && base.Equals(Obj));
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
