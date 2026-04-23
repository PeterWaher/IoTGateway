using SkiaSharp;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Drawing state for text.
	/// </summary>
	public class FontState
	{
		private readonly SKPaint pen;
		private readonly SKFont font;

		/// <summary>
		/// Drawing state for text.
		/// </summary>
		/// <param name="Pen">Pen state</param>
		/// <param name="Font">Font state</param>
		public FontState(SKPaint Pen, SKFont Font)
		{
			this.pen = Pen;
			this.font = Font;
		}

		/// <summary>
		/// Pen state
		/// </summary>
		public SKPaint Pen => this.pen;

		/// <summary>
		/// Font state
		/// </summary>
		public SKFont Font => this.font;
	}
}
