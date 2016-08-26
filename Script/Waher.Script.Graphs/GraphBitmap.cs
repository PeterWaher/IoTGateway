using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
		private Bitmap bitmap;
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
			this.bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
		}

		/// <summary>
		/// Handles bitmap-based graphs.
		/// </summary>
		/// <param name="Bitmap">Graph bitmap.</param>
		public GraphBitmap(Bitmap Bitmap)
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

			GraphSettings Settings = new GraphSettings();
			Settings.Width = this.width;
			Settings.Height = this.height;

			Bitmap Bmp = G.CreateBitmap(Settings);
			using (Graphics Canvas = Graphics.FromImage(Bmp))
			{
				Canvas.CompositingMode = CompositingMode.SourceOver;
				Canvas.CompositingQuality = CompositingQuality.HighQuality;
				Canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
				Canvas.SmoothingMode = SmoothingMode.HighQuality;

				Canvas.DrawImage(this.bitmap, 0, 0, this.width, this.height);
			}

			return new GraphBitmap(Bmp);
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

			GraphSettings Settings = new GraphSettings();
			Settings.Width = this.width;
			Settings.Height = this.height;

			Bitmap Bmp = new Bitmap(this.width, this.height, PixelFormat.Format32bppArgb);
			using (Bitmap Bmp2 = G.CreateBitmap(Settings))
			{
				using (Graphics Canvas = Graphics.FromImage(Bmp))
				{
					Canvas.CompositingMode = CompositingMode.SourceOver;
					Canvas.CompositingQuality = CompositingQuality.HighQuality;
					Canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
					Canvas.SmoothingMode = SmoothingMode.HighQuality;

					Canvas.DrawImage(Bmp2, 0, 0, this.width, this.height);
				}
			}

			return new GraphBitmap(Bmp);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <param name="States">State object(s) that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public override Bitmap CreateBitmap(GraphSettings Settings, out object[] States)
		{
			States = new object[0];
			return this.bitmap;
		}

		/// <summary>
		/// The recommended bitmap size of the graph, if such is available.
		/// </summary>
		public override Size? RecommendedBitmapSize
		{
			get
			{
				return new Size(this.width, this.height);
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
		/// <see cref="Object.Equals"/>
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
			if (this.bitmap != null)
			{
				this.bitmap.Dispose();
				this.bitmap = null;
			}
		}
	}
}
