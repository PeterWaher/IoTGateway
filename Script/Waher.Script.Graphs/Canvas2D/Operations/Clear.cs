using System;
using SkiaSharp;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Clears the canvas with the current brush.
	/// </summary>
	public class Clear : ZeroParameters
	{
		/// <summary>
		/// Clears the canvas with the current brush.
		/// </summary>
		public Clear()
			: base()
		{
		}

		/// <inheritdoc/>
		public override void Draw(SKCanvas Canvas, CanvasState State)
		{
			Canvas.Clear(State.FgColor);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is Clear Obj) && base.Equals(Obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.GetType().FullName.GetHashCode();
		}
	}
}
