using System;
using SkiaSharp;

namespace Waher.Script.Graphs
{
	/// <summary>
	/// Contains pixel information
	/// </summary>
	public abstract class PixelInformation
	{
		private readonly byte[] binary;
		private readonly int width;
		private readonly int height;

		/// <summary>
		/// Contains pixel information
		/// </summary>
		/// <param name="Binary">Binary representation of pixels</param>
		/// <param name="Width">Width</param>
		/// <param name="Height">Height</param>
		public PixelInformation(byte[] Binary, int Width, int Height)
		{
			this.binary = Binary;
			this.width = Width;
			this.height = Height;
		}

		/// <summary>
		/// Binary representation of pixels
		/// </summary>
		public byte[] Binary => this.binary;

		/// <summary>
		/// Width
		/// </summary>
		public int Width => this.width;

		/// <summary>
		/// Height
		/// </summary>
		public int Height => this.height;

		/// <summary>
		/// Gets the pixel information from an <see cref="SKImage"/>.
		/// </summary>
		/// <param name="Image">Image</param>
		/// <returns>Pixel information</returns>
		public static PixelInformation FromImage(SKImage Image)
		{
			using (SKData Data = Image.Encode())
			{
				return new PixelInformationPng(Data.ToArray(), Image.Width, Image.Height);
			}
		}

		/// <summary>
		/// Gets the pixel information object from raw pixel data.
		/// </summary>
		/// <param name="ColorType">How pixels are represented</param>
		/// <param name="Binary">Binary representation of pixels</param>
		/// <param name="Width">Width</param>
		/// <param name="Height">Height</param>
		/// <param name="BytesPerRow">Number of bytes per row</param>
		/// <returns>Pixel information</returns>
		public static PixelInformation FromRaw(SKColorType ColorType, byte[] Binary, int Width, int Height, int BytesPerRow)
		{
			return new PixelInformationRaw(ColorType, Binary, Width, Height, BytesPerRow);
		}

		/// <summary>
		/// Creates an <see cref="SKImage"/> image. It must be disposed by the caller.
		/// </summary>
		/// <returns>Image object.</returns>
		public abstract SKImage CreateBitmap();

		/// <summary>
		/// Encodes the pixels into a binary PNG image.
		/// </summary>
		/// <returns>PNG encoded image.</returns>
		public virtual byte[] EncodeAsPng()
		{
			using (SKImage Image = this.CreateBitmap())
			{
				using (SKData Data = Image.Encode())
				{
					return Data.ToArray();
				}
			}
		}

		/// <summary>
		/// Gets raw pixel data.
		/// </summary>
		/// <returns>Raw pixel data.</returns>
		public abstract PixelInformationRaw GetRaw();

	}
}
