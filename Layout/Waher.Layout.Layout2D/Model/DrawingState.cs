using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Current drawing state.
	/// </summary>
	public class DrawingState : IDisposable
	{
		private Dictionary<string, ILayoutElement> elementsById = new Dictionary<string, ILayoutElement>();
		private Variables session;
		private SKCanvas canvas;
		private SKPaint font;
		private SKPaint fontRoot;
		private SKPaint pen;
		private SKPaint fill;
		private double pixelsPerInch;
		private float? width_0;
		private float? height_0;
		private int viewportWidth;
		private int viewportHeight;

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
			this.viewportWidth = Settings.Width;
			this.viewportHeight = Settings.Height;

			this.font = this.fontRoot = new SKPaint()
			{
				FilterQuality = SKFilterQuality.High,
				HintingLevel = SKPaintHinting.Full,
				SubpixelText = true,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = Settings.TextColor,
				Typeface = SKTypeface.FromFamilyName(Settings.FontName, SKFontStyle.Normal),
				TextSize = (float)(Settings.FontSize * this.pixelsPerInch / 72)
			};

			this.pen = new SKPaint()
			{
				FilterQuality = SKFilterQuality.High,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = Settings.PenColor
			};

			this.fill = new SKPaint()
			{
				FilterQuality = SKFilterQuality.High,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = Settings.BackgroundColor
			};
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.font?.Dispose();
			this.pen?.Dispose();
			this.fill?.Dispose();
		}

		/// <summary>
		/// Current session.
		/// </summary>
		public Variables Session => this.session;

		/// <summary>
		/// Current drawing canvas.
		/// </summary>
		public SKCanvas Canvas => this.canvas;


		/// <summary>
		/// Converts a defined length to drawing size.
		/// </summary>
		/// <param name="L">Length</param>
		/// <param name="Element">Current element.</param>
		/// <param name="Horizontal">If it is a horizontal size.</param>
		/// <returns>Drawing size, if defined.</returns>
		public double GetDrawingSize(Length L, ILayoutElement Element, bool Horizontal)
		{
			switch (L.Unit)
			{
				// pixels (1px = 1/96th of 1in) (absolute)
				case LengthUnit.Px:
					return L.Value * this.pixelsPerInch / 96;

				// points (1pt = 1/72 of 1in) (absolute)
				case LengthUnit.Pt:
					return L.Value * this.pixelsPerInch / 72;

				// picas (1pc = 12 pt) (absolute)
				case LengthUnit.Pc:
					return L.Value * this.pixelsPerInch / 12;

				// centimeters (absolute)
				case LengthUnit.Cm:
					return L.Value * this.pixelsPerInch / 2.54;

				// inches (1in = 96px = 2.54cm)
				case LengthUnit.In:
					return L.Value * this.pixelsPerInch;

				// millimeters (absolute)
				case LengthUnit.Mm:
					return L.Value * this.pixelsPerInch / 25.4;

				// Relative to the font-size of the element (2em means 2 times the size of the current font)
				case LengthUnit.Em:
					return L.Value * this.font.TextSize;

				// Relative to the x-height of the current font (rarely used)
				case LengthUnit.Ex:
					if (!this.height_0.HasValue)
						this.height_0 = this.font.MeasureText("x");	// TODO: Measure height, not width

					return L.Value * this.height_0.Value;

				// Relative to the width of the "0" (zero)
				case LengthUnit.Ch:
					if (!this.width_0.HasValue)
						this.width_0 = this.font.MeasureText("0");

					return L.Value * this.width_0.Value;

				// Relative to font-size of the root element
				case LengthUnit.Rem:
					return L.Value * this.fontRoot.TextSize;

				// Relative to 1% of the width of the viewport
				case LengthUnit.Vw:
					return 0.01 * L.Value * this.viewportWidth;

				// Relative to 1% of the height of the viewport
				case LengthUnit.Vh:
					return 0.01 * L.Value * this.viewportHeight;

				// Relative to 1% of viewport's* smaller dimension
				case LengthUnit.Vmin:
					if (this.viewportWidth < this.viewportHeight)
						return 0.01 * L.Value * this.viewportWidth;
					else
						return 0.01 * L.Value * this.viewportHeight;

				// Relative to 1% of viewport's* larger dimension
				case LengthUnit.Vmax:
					if (this.viewportWidth > this.viewportHeight)
						return 0.01 * L.Value * this.viewportWidth;
					else
						return 0.01 * L.Value * this.viewportHeight;

				// Relative to the parent element
				case LengthUnit.Percent:
					double? Size;

					Element = Element?.Parent;

					while (!(Element is null))
					{
						if (Element is LayoutArea Area)
						{
							if (Horizontal)
								Size = Area.GetWidthEstimate(this);
							else
								Size = Area.GetHeightEstimate(this);

							if (Size.HasValue)
								return L.Value * Size.Value * 0.01;
						}

						Element = Element.Parent;
					}

					if (Horizontal)
						return L.Value * this.viewportWidth * 0.01;
					else
						return L.Value * this.viewportHeight * 0.01;

				default:
					return L.Value;
			}
		}

		/// <summary>
		/// Adds an element with an ID
		/// </summary>
		/// <param name="Id">Element ID</param>
		/// <param name="Element">Element</param>
		public void AddElementId(string Id, ILayoutElement Element)
		{
			this.elementsById[Id] = Element;
		}

		/// <summary>
		/// Tries to get a layout element, given an ID reference
		/// </summary>
		/// <param name="Id">Layout ID</param>
		/// <param name="Element">Element retrieved, if found.</param>
		/// <returns>If an element with the corresponding ID was found.</returns>
		public bool TryGetElement(string Id, out ILayoutElement Element)
		{
			return this.elementsById.TryGetValue(Id, out Element);
		}

		/// <summary>
		/// Clears registered elements with IDs.
		/// </summary>
		public void ClearElementIds()
		{
			this.elementsById.Clear();
		}
	}
}
