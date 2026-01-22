using SkiaSharp;
using System;
using System.Collections.Generic;
using Waher.Layout.Layout2D.Model.Fonts;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Delegate to methods that return a string from a layout element.
	/// </summary>
	public delegate string ElementToString(ILayoutElement Element);

	/// <summary>
	/// Current drawing state.
	/// </summary>
	public class DrawingState : IDisposable
	{
		private Dictionary<ILayoutElement, bool> relativeElements = null;
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
		private bool logRelativeElements = false;

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
					?? SKTypeface.Default
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
		/// <param name="Element">Element being measured.</param>
		public void CalcDrawingSize(Length L, ref float Size, bool Horizontal, ILayoutElement Element)
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
						this.ReportMeasureRelative(Element);
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
						this.ReportMeasureRelative(Element);
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
						this.ReportMeasureRelative(Element);
						Size = Size2;
					}
					break;

				// Relative to font-size of the root element
				case LengthUnit.Rem:
					Size2 = L.Value * this.textRoot.TextSize;
					if (Size != Size2)
					{
						this.ReportMeasureRelative(Element);
						Size = Size2;
					}
					break;

				// Relative to 1% of the width of the viewport
				case LengthUnit.Vw:
					Size2 = L.Value * this.viewportSize.Width / 100;
					if (Size != Size2)
					{
						this.ReportMeasureRelative(Element);
						Size = Size2;
					}
					break;

				// Relative to 1% of the height of the viewport
				case LengthUnit.Vh:
					Size2 = L.Value * this.viewportSize.Height / 100;
					if (Size != Size2)
					{
						this.ReportMeasureRelative(Element);
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
						this.ReportMeasureRelative(Element);
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
						this.ReportMeasureRelative(Element);
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
						this.ReportMeasureRelative(Element);
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

		/// <summary>
		/// Pushes new font settings.
		/// </summary>
		/// <param name="Font">New font, or null if the current font is to be kept.</param>
		/// <returns>State backup, to be used in a call to <see cref="Restore"/>.</returns>
		public FontState Push(Font Font)
		{
			if (Font is null)
				return null;
			else
				return new FontState(Font.Text, Font.FontDef);
		}

		/// <summary>
		/// Restores font settings, from a previous call to <see cref="Push"/>
		/// </summary>
		/// <param name="FontState">Previous font state.</param>
		public void Restore(FontState FontState)
		{
			if (!(FontState is null))
			{
				this.text = FontState.Paint;
				this.font = FontState.Font;
			}
		}

		/// <summary>
		/// If layout contains relative sizes and dimensions should be recalculated.
		/// </summary>
		public bool MeasureRelative
		{
			get;
			private set;
		}

		/// <summary>
		/// Relative elements.
		/// </summary>
		public IEnumerable<ILayoutElement> RelativeElements => this.relativeElements.Keys;

		/// <summary>
		/// Reports an element as having relative measurements.
		/// </summary>
		public void ReportMeasureRelative(ILayoutElement Element)
		{
			this.MeasureRelative = true;
			if (this.logRelativeElements)
				this.relativeElements[Element] = true;
		}

		/// <summary>
		/// Clears information about first relative measurement.
		/// </summary>
		/// <param name="LogRelativeElements">If relative elements should be logged.</param>
		public void ClearRelativeMeasurement(bool LogRelativeElements)
		{
			this.MeasureRelative = false;
			this.logRelativeElements = LogRelativeElements;
			if (this.logRelativeElements)
			{
				if (this.relativeElements is null)
					this.relativeElements = new Dictionary<ILayoutElement, bool>();
				else
					this.relativeElements.Clear();
			}
		}

		/// <summary>
		/// Gets the shortest relative measurements element, given an element to string mapping.
		/// </summary>
		/// <param name="Map">Maps an element to a string.</param>
		public string GetShortestRelativeMeasurement(ElementToString Map)
		{
			string Best = string.Empty;
			int BestLen = int.MaxValue;

			foreach (ILayoutElement Element in this.relativeElements.Keys)
			{
				string Item = Map(Element);
				int Len = Item.Length;

				if (Len < BestLen)
				{
					Best = Item;
					BestLen = Len;
				}
			}

			return Best;
		}

		/// <summary>
		/// Gets the shortest subtree XML of an element with relative measurements.
		/// </summary>
		public string GetShortestRelativeMeasurementXml()
		{
			return this.GetShortestRelativeMeasurement((E) => E.ToXml());
		}

		/// <summary>
		/// Gets the shortest subtree State XML of an element with relative measurements.
		/// </summary>
		public string GetShortestRelativeMeasurementStateXml()
		{
			return this.GetShortestRelativeMeasurement((E) => E.ExportState());
		}
	}
}
