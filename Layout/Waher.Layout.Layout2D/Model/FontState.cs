using System;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Drawing state for text.
	/// </summary>
	public class FontState
	{
		private readonly SKPaint paint;
		private readonly SKFont font;

		/// <summary>
		/// Drawing state for text.
		/// </summary>
		/// <param name="Paint">Paint state</param>
		/// <param name="Font">Font state</param>
		public FontState(SKPaint Paint, SKFont Font)
		{
			this.paint = Paint;
			this.font = Font;
		}

		/// <summary>
		/// Paint state
		/// </summary>
		public SKPaint Paint => this.paint;

		/// <summary>
		/// Font state
		/// </summary>
		public SKFont Font => this.font;
	}
}
