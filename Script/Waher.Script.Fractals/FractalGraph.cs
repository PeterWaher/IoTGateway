using System;
using System.IO;
using SkiaSharp;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Fractals
{
	/// <summary>
	/// Generates new script when zoomed.
	/// </summary>
	/// <param name="r">Real part</param>
	/// <param name="i">Imaginary part</param>
	/// <param name="Size">Current size.</param>
	/// <param name="State">State object.</param>
	public delegate string FractalZoomScript(double r, double i, double Size, object State);

	/// <summary>
	/// Defines a clickable fractal graph in the complex plane.
	/// </summary>
	public class FractalGraph : GraphBitmap
	{
		//private const double LimitPercentChange = 0.0025;
		private const double LimitPercentChange = 0.00025;

		private FractalZoomScript fractalZoomScript;
		private ScriptNode node;
		private double r0, i0, r1, i1, size;
		private object state;
		private int width;
		private int height;
		private bool invertY;

		/// <summary>
		/// Defines a clickable fractal graph in the complex plane.
		/// </summary>
		/// <param name="Image">Fractal image.</param>
		/// <param name="r0">Real part of upper left hand corner.</param>
		/// <param name="i0">Imaginary part of upper left hand corner.</param>
		/// <param name="r1">Real part of lower right hand corner.</param>
		/// <param name="i1">Imaginary part of lower right hand corner.</param>
		/// <param name="Size">Current size.</param>
		/// <param name="InvertY">If the imaginary axis (y-axis) should be inverted or not.</param>
		/// <param name="Node">Script node generating the graph.</param>
		/// <param name="FractalZoomScript">Fractal Zoom Script</param>
		/// <param name="State">State objects.</param>
		public FractalGraph(SKImage Image, double r0, double i0, double r1, double i1, double Size,
			bool InvertY, ScriptNode Node, FractalZoomScript FractalZoomScript, object State)
			: base(Image)
		{
			this.r0 = r0;
			this.i0 = i0;
			this.r1 = r1;
			this.i1 = i1;
			this.state = State;
			this.size = Size;
			this.invertY = InvertY;
			this.width = Image.Width;
			this.height = Image.Height;
			this.node = Node;
			this.fractalZoomScript = FractalZoomScript;
		}

		/// <summary>
		/// <see cref="Graph.GetImageClickScript"/>
		/// </summary>
		public override string GetBitmapClickScript(double X, double Y, object[] States)
		{
			double r = X * (this.r1 - this.r0) / this.width + this.r0;
			double i;

			if (this.invertY)
				i = this.i0 - Y * (this.i0 - this.i1) / this.height;
			else
				i = this.i1 - Y * (this.i1 - this.i0) / this.height;

			if (this.fractalZoomScript == null)
				return "[" + Expression.ToString(r) + ";" + Expression.ToString(i) + "]";

			return this.fractalZoomScript(r, i, this.size, this.state);
		}

		public static SKColor[] ToPalette(ObjectVector Vector)
		{
			int i, c = Vector.Dimension;
			SKColor[] Palette = new SKColor[c];

			for (i = 0; i < c; i++)
				Palette[i] = Graph.ToColor(Vector.GetElement(i).AssociatedObjectValue);

			return Palette;
		}

		public static void Smooth(double[] ColorIndex, double[] Boundary, int Width, int Height, int N,
			SKColor[] Palette, ScriptNode Node, Variables Variables)
		{
			// Passing ColorIndex through the heat equation of 2 spatial dimensions, 
			// maintaining the boundary values fixed in each step.
			//
			// du       ( d2u   d2u )
			// -- = a * | --- + --- |
			// dt       ( dx2   dy2 )
			//
			// the following difference equations will be used to estimate the derivatives:
			//
			//         f(x+h)-2f(x)+f(x-h)
			// f"(x) = ------------------- + O(h^2)
			//                 h^2
			//
			// at the edges, we let f"(x)=0.

			int Size = Width * Height;
			double[] Delta = new double[Size];
			double uxx, uyy;
			double d;
			int Iterations = 0;
			int Index;
			int x, y;
			int DynamicPixels = Size;
			double Sum = Size;
			bool DoPreview = Node.Expression.HandlesPreview;
			System.DateTime LastPreview = System.DateTime.Now;
			System.DateTime TP;

			for (Index = 0; Index < Size; Index++)
			{
				if (Boundary[Index] >= 0 || ColorIndex[Index] >= N)
					DynamicPixels--;
			}

			System.DateTime Start = System.DateTime.Now;
			System.TimeSpan Limit = new System.TimeSpan(1, 0, 0);

			while (100 * Sum / DynamicPixels > LimitPercentChange && Iterations < 50000 && (System.DateTime.Now - Start) < Limit)
			{
				Sum = 0;

				for (y = Index = 0; y < Height; y++)
				{
					for (x = 0; x < Width; x++)
					{
						d = Boundary[Index];
						if (d >= 0)
						{
							Delta[Index++] = 0;
							continue;
						}

						d = 2 * ColorIndex[Index];
						if (x == 0 || x == Width - 1)
							uxx = 0;
						else
							uxx = ColorIndex[Index - 1] - d + ColorIndex[Index + 1];

						if (y == 0 || y == Height - 1)
							uyy = 0;
						else
							uyy = ColorIndex[Index - Width] - d + ColorIndex[Index + Width];

						d = 0.2 * (uxx + uyy);
						Delta[Index++] = d;
						Sum += Math.Abs(d);
					}
				}

				for (Index = 0; Index < Size; Index++)
					ColorIndex[Index] += Delta[Index];

				Iterations++;

				TP = System.DateTime.Now;
				if ((TP - LastPreview).TotalSeconds > 5)
				{
					if (Node != null)
					{
						LastPreview = TP;

						if (DoPreview)
							Node.Expression.Preview(new GraphBitmap(
								FractalGraph.ToBitmap(ColorIndex, Width, Height, Palette)));

						Node.Expression.Status("Smoothing. Change: " + (100 * Sum / DynamicPixels).ToString("F3") + "%, Limit: " + LimitPercentChange.ToString("F3") + "%, Iterations: " + Iterations.ToString());
					}
				}
			}

			Variables.ConsoleOut.Write("Iterations: " + Iterations.ToString());
			Node.Expression.Status(string.Empty);
		}

		public static double[] FindBoundaries(double[] ColorIndex, int Width, int Height)
		{
			// Finding boundary values:

			double[] Boundary = (double[])ColorIndex.Clone();
			double d, d2;
			int Index;
			int x, y;

			Index = 0;

			d = ColorIndex[0];

			d2 = ColorIndex[1];
			if (d <= d2 && d > d2 - 2)
			{
				d2 = ColorIndex[Width];
				if (d <= d2 && d > d2 - 2)
					Boundary[0] = -1;
			}

			Index++;

			for (x = 2; x < Width; x++, Index++)
			{
				d = ColorIndex[Index];

				d2 = ColorIndex[Index + 1];
				if (d > d2 || d <= d2 - 2)
					continue;

				d2 = ColorIndex[Index - 1];
				if (d > d2 || d <= d2 - 2)
					continue;

				d2 = ColorIndex[Index + Width];
				if (d > d2 || d <= d2 - 2)
					continue;

				Boundary[Index] = -1;
			}

			d2 = ColorIndex[Index];
			if (d <= d2 && d > d2 - 2)
			{
				d2 = ColorIndex[Index - 1];
				if (d <= d2 && d > d2 - 2)
				{
					d2 = ColorIndex[Index + Width];
					if (d <= d2 && d > d2 - 2)
						Boundary[0] = -1;
				}
			}

			Index++;

			for (y = 2; y < Height; y++)
			{
				d = ColorIndex[Index];

				d2 = ColorIndex[Index + 1];
				if (d <= d2 && d > d2 - 2)
				{
					d2 = ColorIndex[Index - Width];
					if (d <= d2 && d > d2 - 2)
					{
						d2 = ColorIndex[Index + Width];
						if (d <= d2 && d > d2 - 2)
							Boundary[0] = -1;
					}
				}

				Index++;

				for (x = 2; x < Width; x++, Index++)
				{
					d = ColorIndex[Index];

					d2 = ColorIndex[Index + 1];
					if (d > d2 || d <= d2 - 2)
						continue;

					d2 = ColorIndex[Index - 1];
					if (d > d2 || d <= d2 - 2)
						continue;

					d2 = ColorIndex[Index + Width];
					if (d > d2 || d <= d2 - 2)
						continue;

					d2 = ColorIndex[Index - Width];
					if (d > d2 || d <= d2 - 2)
						continue;

					Boundary[Index] = -1;
				}

				d = ColorIndex[Index];

				d2 = ColorIndex[Index - 1];
				if (d <= d2 && d > d2 - 2)
				{
					d2 = ColorIndex[Index - Width];
					if (d <= d2 && d > d2 - 2)
					{
						d2 = ColorIndex[Index + Width];
						if (d <= d2 && d > d2 - 2)
							Boundary[0] = -1;
					}
				}

				Index++;
			}

			d = ColorIndex[Index];

			d2 = ColorIndex[Index + 1];
			if (d <= d2 && d > d2 - 2)
			{
				d2 = ColorIndex[Index - Width];
				if (d <= d2 && d > d2 - 2)
					Boundary[0] = -1;
			}

			Index++;

			for (x = 2; x < Width; x++, Index++)
			{
				d = ColorIndex[Index];

				d2 = ColorIndex[Index + 1];
				if (d > d2 || d <= d2 - 2)
					continue;

				d2 = ColorIndex[Index - 1];
				if (d > d2 || d <= d2 - 2)
					continue;

				d2 = ColorIndex[Index - Width];
				if (d > d2 || d <= d2 - 2)
					continue;

				Boundary[Index] = -1;
			}

			d = ColorIndex[Index];

			d2 = ColorIndex[Index - 1];
			if (d <= d2 && d > d2 - 2)
			{
				d2 = ColorIndex[Index - Width];
				if (d <= d2 && d > d2 - 2)
					Boundary[0] = -1;
			}

			return Boundary;
		}

		public static int[] FindBoundaries(int[] ColorIndex, int Width, int Height)
		{
			// Finding boundary values:

			int[] Boundary = (int[])ColorIndex.Clone();
			int Index;
			int d, d2;
			int x, y;

			Index = 0;

			d = ColorIndex[0];

			d2 = ColorIndex[1];
			if (d <= d2 && d > d2 - 2)
			{
				d2 = ColorIndex[Width];
				if (d <= d2 && d > d2 - 2)
					Boundary[0] = -1;
			}

			Index++;

			for (x = 2; x < Width; x++, Index++)
			{
				d = ColorIndex[Index];

				d2 = ColorIndex[Index + 1];
				if (d > d2 || d <= d2 - 2)
					continue;

				d2 = ColorIndex[Index - 1];
				if (d > d2 || d <= d2 - 2)
					continue;

				d2 = ColorIndex[Index + Width];
				if (d > d2 || d <= d2 - 2)
					continue;

				Boundary[Index] = -1;
			}

			d2 = ColorIndex[Index];
			if (d <= d2 && d > d2 - 2)
			{
				d2 = ColorIndex[Index - 1];
				if (d <= d2 && d > d2 - 2)
				{
					d2 = ColorIndex[Index + Width];
					if (d <= d2 && d > d2 - 2)
						Boundary[0] = -1;
				}
			}

			Index++;

			for (y = 2; y < Height; y++)
			{
				d = ColorIndex[Index];

				d2 = ColorIndex[Index + 1];
				if (d <= d2 && d > d2 - 2)
				{
					d2 = ColorIndex[Index - Width];
					if (d <= d2 && d > d2 - 2)
					{
						d2 = ColorIndex[Index + Width];
						if (d <= d2 && d > d2 - 2)
							Boundary[0] = -1;
					}
				}

				Index++;

				for (x = 2; x < Width; x++, Index++)
				{
					d = ColorIndex[Index];

					d2 = ColorIndex[Index + 1];
					if (d > d2 || d <= d2 - 2)
						continue;

					d2 = ColorIndex[Index - 1];
					if (d > d2 || d <= d2 - 2)
						continue;

					d2 = ColorIndex[Index + Width];
					if (d > d2 || d <= d2 - 2)
						continue;

					d2 = ColorIndex[Index - Width];
					if (d > d2 || d <= d2 - 2)
						continue;

					Boundary[Index] = -1;
				}

				d = ColorIndex[Index];

				d2 = ColorIndex[Index - 1];
				if (d <= d2 && d > d2 - 2)
				{
					d2 = ColorIndex[Index - Width];
					if (d <= d2 && d > d2 - 2)
					{
						d2 = ColorIndex[Index + Width];
						if (d <= d2 && d > d2 - 2)
							Boundary[0] = -1;
					}
				}

				Index++;
			}

			d = ColorIndex[Index];

			d2 = ColorIndex[Index + 1];
			if (d <= d2 && d > d2 - 2)
			{
				d2 = ColorIndex[Index - Width];
				if (d <= d2 && d > d2 - 2)
					Boundary[0] = -1;
			}

			Index++;

			for (x = 2; x < Width; x++, Index++)
			{
				d = ColorIndex[Index];

				d2 = ColorIndex[Index + 1];
				if (d > d2 || d <= d2 - 2)
					continue;

				d2 = ColorIndex[Index - 1];
				if (d > d2 || d <= d2 - 2)
					continue;

				d2 = ColorIndex[Index - Width];
				if (d > d2 || d <= d2 - 2)
					continue;

				Boundary[Index] = -1;
			}

			d = ColorIndex[Index];

			d2 = ColorIndex[Index - 1];
			if (d <= d2 && d > d2 - 2)
			{
				d2 = ColorIndex[Index - Width];
				if (d <= d2 && d > d2 - 2)
					Boundary[0] = -1;
			}

			return Boundary;
		}

		public static SKImage ToBitmap(double[] ColorIndex, int Width, int Height, SKColor[] Palette)
		{
			int N = Palette.Length;
			int Size = Width * Height;
			int Size4 = Size * 4;
			byte[] rgb = new byte[Size4];
			byte[] reds;
			byte[] greens;
			byte[] blues;
			double d;
			SKColor cl;
			int Index2;
			int ci;
			int Component;
			int Index;
			int x;

			reds = new byte[N];
			greens = new byte[N];
			blues = new byte[N];

			for (x = 0; x < N; x++)
			{
				cl = Palette[x];
				reds[x] = cl.Red;
				greens[x] = cl.Green;
				blues[x] = cl.Blue;
			}

			for (Index = Index2 = 0; Index < Size; Index++)
			{
				d = ColorIndex[Index];

				ci = (int)d;
				if (ci < 0 || ci >= N)
				{
					rgb[Index2++] = 0;
					rgb[Index2++] = 0;
					rgb[Index2++] = 0;
					rgb[Index2++] = 255;
				}
				else if (ci == N - 1)
				{
					rgb[Index2++] = blues[ci];
					rgb[Index2++] = greens[ci];
					rgb[Index2++] = reds[ci];
					rgb[Index2++] = 255;
				}
				else
				{
					d -= ci;

					Component = (int)(blues[ci + 1] * d + blues[ci] * (1 - d) + 0.5);
					if (Component > 255)
						rgb[Index2++] = 255;
					else
						rgb[Index2++] = (byte)Component;

					Component = (int)(greens[ci + 1] * d + greens[ci] * (1 - d) + 0.5);
					if (Component > 255)
						rgb[Index2++] = 255;
					else
						rgb[Index2++] = (byte)Component;

					Component = (int)(reds[ci + 1] * d + reds[ci] * (1 - d) + 0.5);
					if (Component > 255)
						rgb[Index2++] = 255;
					else
						rgb[Index2++] = (byte)Component;

					rgb[Index2++] = 255;
				}
			}

			using (SKData Data = SKData.Create(new MemoryStream(rgb)))
			{
				return SKImage.FromPixelData(new SKImageInfo(Width, Height, SKColorType.Bgra8888), Data, Width * 4);
			}
		}

		public static SKImage ToBitmap(int[] ColorIndex, int Width, int Height, SKColor[] Palette)
		{
			int N = Palette.Length;
			byte[] reds = new byte[N];
			byte[] greens = new byte[N];
			byte[] blues = new byte[N];
			SKColor cl;
			int x;

			for (x = 0; x < N; x++)
			{
				cl = Palette[x];
				reds[x] = cl.Red;
				greens[x] = cl.Green;
				blues[x] = cl.Blue;
			}

			int Size = Width * Height;
			int Size4 = Size * 4;
			byte[] rgb = new byte[Size4];
			int Index, Index2;
			int d;

			for (Index = Index2 = 0; Index < Size; Index++)
			{
				d = ColorIndex[Index];

				if (d < 0 || d >= N)
				{
					rgb[Index2++] = 0;
					rgb[Index2++] = 0;
					rgb[Index2++] = 0;
					rgb[Index2++] = 255;
				}
				else
				{
					rgb[Index2++] = blues[d];
					rgb[Index2++] = greens[d];
					rgb[Index2++] = reds[d];
					rgb[Index2++] = 255;
				}
			}

			using (SKData Data = SKData.Create(new MemoryStream(rgb)))
			{
				return SKImage.FromPixelData(new SKImageInfo(Width, Height, SKColorType.Bgra8888), Data, Width * 4);
			}
		}

	}
}
