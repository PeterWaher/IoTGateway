using System;
using System.Runtime.InteropServices;
using SkiaSharp;
using System.Text;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Graphs
{
	/// <summary>
	/// Handles bitmap-based graphs.
	/// </summary>
	public class GraphBitmap : Graph, IDisposable
	{
		private SKImage bitmap;
		private int width;
		private int height;

		/// <summary>
		/// Handles bitmap-based graphs.
		/// </summary>
		/// <param name="Width">Width of graph, in pixels.</param>
		/// <param name="Height">Height of graph, in pixels.</param>
		public GraphBitmap(int Width, int Height)
			: base()
		{
			this.width = Width;
			this.height = Height;
			this.bitmap = null;
		}

		/// <summary>
		/// Handles bitmap-based graphs.
		/// </summary>
		/// <param name="Bitmap">Graph bitmap.</param>
		public GraphBitmap(SKImage Bitmap)
			: base()
		{
			this.width = Bitmap.Width;
			this.height = Bitmap.Height;
			this.bitmap = Bitmap;
		}

		/// <summary>
		/// Tries to add an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ISemiGroupElement AddLeft(ISemiGroupElement Element)
		{
			Graph G = Element as Graph;
			if (G == null)
				return null;

			GraphSettings Settings = new GraphSettings()
			{
				Width = this.width,
				Height = this.height
			};

			SKImage Bmp = G.CreateBitmap(Settings);

			if (this.bitmap == null)
				return new GraphBitmap(Bmp);

			using (SKSurface Surface = SKSurface.Create(Math.Max(Bmp.Width, this.width), Math.Max(Bmp.Height, this.height),
				SKImageInfo.PlatformColorType, SKAlphaType.Premul))
			{
				SKCanvas Canvas = Surface.Canvas;

				Canvas.DrawImage(Bmp, 0, 0);
				Canvas.DrawImage(this.bitmap, 0, 0);

				Bmp.Dispose();

				return new GraphBitmap(Surface.Snapshot());
			}
		}

		/// <summary>
		/// Tries to add an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ISemiGroupElement AddRight(ISemiGroupElement Element)
		{
			Graph G = Element as Graph;
			if (G == null)
				return null;

			GraphSettings Settings = new GraphSettings()
			{
				Width = this.width,
				Height = this.height
			};

			SKImage Bmp = G.CreateBitmap(Settings);

			if (this.bitmap == null)
				return new GraphBitmap(Bmp);

			using (SKSurface Surface = SKSurface.Create(Math.Max(Bmp.Width, this.width), Math.Max(Bmp.Height, this.height),
				SKImageInfo.PlatformColorType, SKAlphaType.Premul))
			{
				SKCanvas Canvas = Surface.Canvas;

				Canvas.DrawImage(this.bitmap, 0, 0);
				Canvas.DrawImage(Bmp, 0, 0);

				Bmp.Dispose();

				return new GraphBitmap(Surface.Snapshot());
			}
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <param name="States">State object(s) that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public override SKImage CreateBitmap(GraphSettings Settings, out object[] States)
		{
			SKImageInfo ImageInfo = new SKImageInfo(this.bitmap.Width, this.bitmap.Height, SKColorType.Bgra8888);
			int c = ImageInfo.BytesSize;

			States = new object[0];

			IntPtr Pixels = Marshal.AllocCoTaskMem(c);
			try
			{
				this.bitmap.ReadPixels(ImageInfo, Pixels, ImageInfo.RowBytes, 0, 0);

				using (SKData Data = SKData.Create(Pixels, c))
				{
					SKImage Result = SKImage.FromPixelData(new SKImageInfo(ImageInfo.Width, ImageInfo.Height, SKColorType.Bgra8888), Data, ImageInfo.RowBytes);
					Pixels = IntPtr.Zero;

					return Result;
				}
			}
			finally
			{
				if (Pixels != IntPtr.Zero)
					Marshal.FreeCoTaskMem(Pixels);
			}
		}

		/// <summary>
		/// The recommended bitmap size of the graph, if such is available.
		/// </summary>
		public override Tuple<int, int> RecommendedBitmapSize
		{
			get
			{
				return new Tuple<int, int>(this.width, this.height);
			}
		}

		/// <summary>
		/// Gets script corresponding to a point in a generated bitmap representation of the graph.
		/// </summary>
		/// <param name="X">X-Coordinate.</param>
		/// <param name="Y">Y-Coordinate.</param>
		/// <param name="States">State objects for the generated bitmap.</param>
		/// <returns>Script.</returns>
		public override string GetBitmapClickScript(double X, double Y, object[] States)
		{
			return "[" + Expression.ToString(X) + "," + Expression.ToString(Y) + "]";
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			GraphBitmap B = obj as GraphBitmap;
			return (B != null && this.bitmap.Equals(B.bitmap));
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return this.bitmap.GetHashCode();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
		}
	}
}
