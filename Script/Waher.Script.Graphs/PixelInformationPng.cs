using System;
using SkiaSharp;

namespace Waher.Script.Graphs
{
	/// <summary>
	/// Contains pixel information in PNG format
	/// </summary>
	public class PixelInformationPng : PixelInformation
	{
		/// <summary>
		/// Contains pixel information in PNG format
		/// </summary>
		/// <param name="Binary">Binary representation of pixels</param>
		/// <param name="Width">Width</param>
		/// <param name="Height">Height</param>
		public PixelInformationPng(byte[] Binary, int Width, int Height)
			: base(Binary, Width, Height)
		{
		}

		/// <summary>
		/// Creates an <see cref="SKImage"/> image. It must be disposed by the caller.
		/// </summary>
		/// <returns>Image object.</returns>
		public override SKImage CreateBitmap()
		{
			using (SKBitmap Bitmap = SKBitmap.Decode(this.Binary))
			{
				return SKImage.FromBitmap(Bitmap);
			}
		}

		/// <summary>
		/// Encodes the pixels into a binary PNG image.
		/// </summary>
		/// <returns>PNG encoded image.</returns>
		public override byte[] EncodeAsPng()
		{
			return this.Binary;
		}

		/// <summary>
		/// Gets raw pixel data.
		/// </summary>
		/// <returns>Raw pixel data.</returns>
		public override PixelInformationRaw GetRaw()
		{
			using (SKBitmap Bitmap = SKBitmap.Decode(this.Binary))
			{
				return new PixelInformationRaw(Bitmap.ColorType, Bitmap.Bytes, Bitmap.Width, Bitmap.Height,
					Bitmap.Width * Bitmap.BytesPerPixel);
			}
		}
	}
}
