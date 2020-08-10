using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace Waher.Layout.Layout2D
{
	/// <summary>
	/// Affects the size of the rendered image.
	/// </summary>
	public enum RenderedImageSize
	{
		/// <summary>
		/// Scales the layout to fit the desired image size.
		/// </summary>
		ScaleToFit,

		/// <summary>
		/// Resizes the output to match the size of the layout.
		/// </summary>
		ResizeImage
	}

	/// <summary>
	/// Render settings.
	/// </summary>
	public class RenderSettings
	{
		private RenderedImageSize imageSize = RenderedImageSize.ResizeImage;
		private int width = 800;
		private int height = 600;
		private float zoom = 1;
		private float offsetX = 0;
		private float offsetY = 0;
		private SKColor backgroundColor = SKColors.White;
		private SKColor penColor = SKColors.Black;
		private SKColor textColor = SKColors.Black;
		private string fontName = "Segoe UI";
		private float fontSize = 12;
		private float pixelsPerInch = 96;

		/// <summary>
		/// Determines the size of the rendered image.
		/// </summary>
		public RenderSettings()
		{
		}

		/// <summary>
		/// Offset along X-axis.
		/// </summary>
		public RenderedImageSize ImageSize
		{
			get => this.imageSize;
			set => this.imageSize = value;
		}

		/// <summary>
		/// Width of image
		/// </summary>
		public int Width
		{
			get => this.width;
			set
			{
				if (value <= 0)
					throw new ArgumentException("Invalid width.", nameof(Width));

				this.width = value;
			}
		}

		/// <summary>
		/// Height of image
		/// </summary>
		public int Height
		{
			get => this.height;
			set
			{
				if (value <= 0)
					throw new ArgumentException("Invalid height.", nameof(Height));

				this.height = value;
			}
		}

		/// <summary>
		/// Zoom factor
		/// </summary>
		public float Zoom
		{
			get => this.zoom;
			set
			{
				if (value <= 0)
					throw new ArgumentException("Invalid zoom factor.", nameof(Zoom));

				this.zoom = value;
			}
		}

		/// <summary>
		/// Offset along X-axis.
		/// </summary>
		public float OffsetX
		{
			get => this.offsetX;
			set => this.offsetX = value;
		}

		/// <summary>
		/// Offset along Y-axis.
		/// </summary>
		public float OffsetY
		{
			get => this.offsetY;
			set => this.offsetY = value;
		}

		/// <summary>
		/// Background color
		/// </summary>
		public SKColor BackgroundColor
		{
			get => this.backgroundColor;
			set => this.backgroundColor = value;
		}

		/// <summary>
		/// Pen color
		/// </summary>
		public SKColor PenColor
		{
			get => this.penColor;
			set => this.penColor = value;
		}

		/// <summary>
		/// Text color
		/// </summary>
		public SKColor TextColor
		{
			get => this.textColor;
			set => this.textColor = value;
		}

		/// <summary>
		/// Font name
		/// </summary>
		public string FontName
		{
			get => this.fontName;
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentException("Invalid font name.", nameof(FontName));

				this.fontName = value;
			}
		}

		/// <summary>
		/// Font size, in points
		/// </summary>
		public float FontSize
		{
			get => this.fontSize;
			set
			{
				if (value <= 0)
					throw new ArgumentException("Invalid font size.", nameof(FontSize));

				this.fontSize = value;
			}
		}

		/// <summary>
		/// Pixels per inch (defualt=96 pixels/inch)
		/// </summary>
		public float PixelsPerInch
		{
			get => this.pixelsPerInch;
			set
			{
				if (value <= 0)
					throw new ArgumentException("Invalid number of pixels per inch.", nameof(PixelsPerInch));

				this.pixelsPerInch = value;
			}
		}

	}
}
