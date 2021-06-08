using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Current drawing state in a canvas graph.
	/// </summary>
	public class CanvasState
	{
		private SKColor fgColor;
		private SKColor bgColor;
		private SKPaint pen = null;
		private SKPaint brush = null;
		private float width = 1;

		/// <summary>
		/// Foreground color
		/// </summary>
		public SKColor FgColor
		{
			get => this.fgColor;
			set
			{
				this.fgColor = value;

				this.pen?.Dispose();
				this.pen = null;

				this.brush?.Dispose();
				this.brush = null;
			}
		}

		/// <summary>
		/// Pen width
		/// </summary>
		public float Width
		{
			get => this.width;
			set
			{
				this.width = value;

				this.pen?.Dispose();
				this.pen = null;
			}
		}

		/// <summary>
		/// Background color
		/// </summary>
		public SKColor BgColor
		{
			get => this.bgColor;
			set => this.bgColor = value;
		}

		/// <summary>
		/// Current X-coordinate.
		/// </summary>
		public float X
		{
			get;
			set;
		}

		/// <summary>
		/// Current Y-coordinate.
		/// </summary>
		public float Y
		{
			get;
			set;
		}

		/// <summary>
		/// Current pen
		/// </summary>
		public SKPaint Pen
		{
			get
			{
				if (this.pen is null)
				{
					this.pen = new SKPaint()
					{
						FilterQuality = SKFilterQuality.High,
						IsAntialias = true,
						Style = SKPaintStyle.Stroke,
						Color = this.fgColor,
						StrokeWidth = this.width
					};
				}

				return this.pen;
			}
		}

		/// <summary>
		/// Current brush
		/// </summary>
		public SKPaint Brush
		{
			get
			{
				if (this.brush is null)
				{
					this.brush = new SKPaint()
					{
						FilterQuality = SKFilterQuality.High,
						IsAntialias = true,
						Style = SKPaintStyle.Fill,
						Color = this.fgColor
					};
				}

				return this.brush;
			}
		}
	}
}
