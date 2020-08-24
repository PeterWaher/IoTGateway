using System;
using SkiaSharp;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Current drawing state.
	/// </summary>
	public class DrawingState : IDisposable
	{
		private readonly Variables session;
		private readonly SKPaint textRoot;
		private readonly SKPaint defaultPen;
		private SKCanvas canvas;
		private SKPaint shapePen;
		private SKPaint shapeFill;
		private SKFont font;
		private SKPaint text;
		private float? width_0;
		private float? height_x;
		private SKSize areaSize;
		private SKSize viewportSize;
		private readonly float pixelsPerInch;

		/// <summary>
		/// Current drawing state.
		/// </summary>
		/// <param name="Canvas">Current drawing canvas.</param>
		/// <param name="Settings">Rendering settings.</param>
		/// <param name="Session">Session</param>
		public DrawingState(SKCanvas Canvas, RenderSettings Settings, Variables Session)
		{
			this.canvas = Canvas;
			this.session = Session;
			this.pixelsPerInch = Settings.PixelsPerInch;
			this.areaSize = this.viewportSize = new SKSize(Settings.Width, Settings.Height);

			this.font = new SKFont()
			{
				Edging = SKFontEdging.SubpixelAntialias,
				Hinting = SKFontHinting.Full,
				Subpixel = true,
				Size = (float)(Settings.FontSize * this.pixelsPerInch / 72),
				Typeface = SKTypeface.FromFamilyName(Settings.FontName, SKFontStyle.Normal)
			};

			this.text = this.textRoot = new SKPaint()
			{
				FilterQuality = SKFilterQuality.High,
				HintingLevel = SKPaintHinting.Full,
				SubpixelText = true,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = Settings.TextColor,
				Typeface = this.font.Typeface,
				TextSize = this.font.Size
			};

			this.defaultPen = new SKPaint()
			{
				FilterQuality = SKFilterQuality.High,
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				Color = Settings.PenColor
			};
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.text?.Dispose();
			this.font?.Dispose();
			this.defaultPen?.Dispose();
		}

		/// <summary>
		/// Current session.
		/// </summary>
		public Variables Session => this.session;

		/// <summary>
		/// Pixels per inch
		/// </summary>
		public float PixelsPerInch => this.pixelsPerInch;

		/// <summary>
		/// Current drawing canvas.
		/// </summary>
		public SKCanvas Canvas
		{
			get => this.canvas;
			internal set => this.canvas = value;
		}

		/// <summary>
		/// Current text paint settings
		/// </summary>
		public SKPaint Text
		{
			get => this.text;
			set => this.text = value;
		}

		/// <summary>
		/// Current font.
		/// </summary>
		public SKFont Font
		{
			get => this.font;
			set => this.font = value;
		}

		/// <summary>
		/// Default pen
		/// </summary>
		public SKPaint DefaultPen => this.defaultPen;

		/// <summary>
		/// Pen to use for shape, if no other is specified in the shape.
		/// </summary>
		public SKPaint ShapePen
		{
			get => this.shapePen;
			set => this.shapePen = value;
		}

		/// <summary>
		/// Fill to use for shape, if no other is specified in the shape.
		/// </summary>
		public SKPaint ShapeFill
		{
			get => this.shapeFill;
			set => this.shapeFill = value;
		}

		/// <summary>
		/// Width of current area
		/// </summary>
		public float AreaWidth => this.areaSize.Width;

		/// <summary>
		/// Height of current area
		/// </summary>
		public float AreaHeight => this.areaSize.Height;

		/// <summary>
		/// Converts a defined length to drawing size.
		/// </summary>
		/// <param name="L">Length</param>
		/// <param name="Size">Calculated size.</param>
		/// <param name="Horizontal">If it is a horizontal size.</param>
		/// <param name="Relative">If size is relative, and should be recalculated if dimensions change.</param>
		public void CalcDrawingSize(Length L, ref float Size, bool Horizontal, ref bool Relative)
		{
			switch (L.Unit)
			{
				// pixels (1px = 1/96th of 1in) (absolute)
				case LengthUnit.Px:
					Size = L.Value * this.pixelsPerInch / 96;
					break;

				// points (1pt = 1/72 of 1in) (absolute)
				case LengthUnit.Pt:
					Size = L.Value * this.pixelsPerInch / 72;
					break;

				// picas (1pc = 12 pt) (absolute)
				case LengthUnit.Pc:
					Size = L.Value * this.pixelsPerInch / 12;
					break;

				// centimeters (absolute)
				case LengthUnit.Cm:
					Size = L.Value * this.pixelsPerInch / 2.54f;
					break;

				// inches (1in = 96px = 2.54cm)
				case LengthUnit.In:
					Size = L.Value * this.pixelsPerInch;
					break;

				// millimeters (absolute)
				case LengthUnit.Mm:
					Size = L.Value * this.pixelsPerInch / 25.4f;
					break;

				// Relative to the font-size of the element (2em means 2 times the size of the current font)
				case LengthUnit.Em:
					float Size2 = L.Value * this.text.TextSize;
					if (Size != Size2)
					{
						Relative = true;
						Size = Size2;
					}
					break;

				// Relative to the x-height of the current font (rarely used)
				case LengthUnit.Ex:
					if (!this.height_x.HasValue)
					{
						SKRect Bounds = new SKRect();
						this.text.MeasureText("x", ref Bounds);
						this.height_x = Bounds.Height;
					}

					Size2 = L.Value * this.height_x.Value;
					if (Size != Size2)
					{
						Relative = true;
						Size = Size2;
					}
					break;

				// Relative to the width of the "0" (zero)
				case LengthUnit.Ch:
					if (!this.width_0.HasValue)
						this.width_0 = this.text.MeasureText("0");

					Size2 = L.Value * this.width_0.Value;
					if (Size != Size2)
					{
						Relative = true;
						Size = Size2;
					}
					break;

				// Relative to font-size of the root element
				case LengthUnit.Rem:
					Size2 = L.Value * this.textRoot.TextSize;
					if (Size != Size2)
					{
						Relative = true;
						Size = Size2;
					}
					break;

				// Relative to 1% of the width of the viewport
				case LengthUnit.Vw:
					Size2 = L.Value * this.viewportSize.Width / 100;
					if (Size != Size2)
					{
						Relative = true;
						Size = Size2;
					}
					break;

				// Relative to 1% of the height of the viewport
				case LengthUnit.Vh:
					Size2 = L.Value * this.viewportSize.Height / 100;
					if (Size != Size2)
					{
						Relative = true;
						Size = Size2;
					}
					break;

				// Relative to 1% of viewport's* smaller dimension
				case LengthUnit.Vmin:
					if (this.viewportSize.Width < this.viewportSize.Height)
						Size2 = L.Value * this.viewportSize.Width / 100;
					else
						Size2 = L.Value * this.viewportSize.Height / 100;

					if (Size != Size2)
					{
						Relative = true;
						Size = Size2;
					}
					break;

				// Relative to 1% of viewport's* larger dimension
				case LengthUnit.Vmax:
					if (this.viewportSize.Width > this.viewportSize.Height)
						Size2 = L.Value * this.viewportSize.Width / 100;
					else
						Size2 = L.Value * this.viewportSize.Height / 100;

					if (Size != Size2)
					{
						Relative = true;
						Size = Size2;
					}
					break;

				// Relative to the parent element
				case LengthUnit.Percent:
					if (Horizontal)
						Size2 = L.Value * this.areaSize.Width / 100;
					else
						Size2 = L.Value * this.areaSize.Height / 100;

					if (Size != Size2)
					{
						Relative = true;
						Size = Size2;
					}
					break;

				default:
					Size = L.Value;
					break;
			}
		}

		/// <summary>
		/// Sets the current area size.
		/// </summary>
		/// <param name="AreaSize">New area size</param>
		/// <returns>Previous area size</returns>
		public SKSize SetAreaSize(SKSize AreaSize)
		{
			SKSize Prev = this.areaSize;
			this.areaSize = AreaSize;
			return Prev;
		}
	}
}
