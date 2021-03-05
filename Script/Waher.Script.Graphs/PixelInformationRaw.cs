using System;
using System.IO;
using SkiaSharp;

namespace Waher.Script.Graphs
{
	/// <summary>
	/// Contains pixel information in a raw unencoded format.
	/// </summary>
	public class PixelInformationRaw : PixelInformation
	{
		private readonly SKColorType colorType;
		private readonly int bytesPerRow;

		/// <summary>
		/// Contains pixel information in a raw unencoded format.
		/// </summary>
		/// <param name="ColorType">How pixels are represented</param>
		/// <param name="Binary">Binary representation of pixels</param>
		/// <param name="Width">Width</param>
		/// <param name="Height">Height</param>
		/// <param name="BytesPerRow">Number of bytes per row</param>
		public PixelInformationRaw(SKColorType ColorType, byte[] Binary, int Width, int Height, int BytesPerRow)
			: base(Binary, Width, Height)
		{
			this.colorType = ColorType;
			this.bytesPerRow = BytesPerRow;
		}

		/// <summary>
		/// How pixels are represented
		/// </summary>
		public SKColorType ColorType => this.colorType;

		/// <summary>
		/// Number of bytes per row
		/// </summary>
		public int BytesPerRow => this.bytesPerRow;

		/// <summary>
		/// Creates an <see cref="SKImage"/> image. It must be disposed by the caller.
		/// </summary>
		/// <returns>Image object.</returns>
		public override SKImage CreateBitmap()
		{
			using (MemoryStream ms = new MemoryStream(this.Binary))
			{
				using (SKData Data = SKData.Create(ms))
				{
					return SKImage.FromPixels(new SKImageInfo(this.Width, this.Height, this.colorType), Data, this.bytesPerRow);
				}
			}
		}

		/// <summary>
		/// Gets raw pixel data.
		/// </summary>
		/// <returns>Raw pixel data.</returns>
		public override PixelInformationRaw GetRaw()
		{
			return this;
		}
	}
}
