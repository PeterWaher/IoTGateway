using System;
using SkiaSharp;
using Waher.Script.Graphs;
using Waher.Script.Model;
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

		private readonly FractalZoomScript fractalZoomScript;
		private readonly ScriptNode node;
		private readonly double r0, i0, r1, i1, size;
		private readonly object state;
		private readonly int width;
		private readonly int height;
		private readonly bool invertY;

		/// <summary>
		/// Defines a clickable fractal graph in the complex plane.
		/// </summary>
		public FractalGraph()
			: base()
		{
		}

		/// <summary>
		/// Defines a clickable fractal graph in the complex plane.
		/// </summary>
		/// <param name="Pixels">Fractal image.</param>
		/// <param name="r0">Real part of upper left hand corner.</param>
		/// <param name="i0">Imaginary part of upper left hand corner.</param>
		/// <param name="r1">Real part of lower right hand corner.</param>
		/// <param name="i1">Imaginary part of lower right hand corner.</param>
		/// <param name="Size">Current size.</param>
		/// <param name="InvertY">If the imaginary axis (y-axis) should be inverted or not.</param>
		/// <param name="Node">Script node generating the graph.</param>
		/// <param name="FractalZoomScript">Fractal Zoom Script</param>
		/// <param name="State">State objects.</param>
		public FractalGraph(PixelInformation Pixels, double r0, double i0, double r1, double i1, double Size,
			bool InvertY, ScriptNode Node, FractalZoomScript FractalZoomScript, object State)
			: base(Pixels)
		{
			this.r0 = r0;
			this.i0 = i0;
			this.r1 = r1;
			this.i1 = i1;
			this.state = State;
			this.size = Size;
			this.invertY = InvertY;
			this.width = Pixels.Width;
			this.height = Pixels.Height;
			this.node = Node;
			this.fractalZoomScript = FractalZoomScript;
		}

		/// <summary>
		/// Node generating the graph.
		/// </summary>
		public ScriptNode Node => this.node;

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

			if (this.fractalZoomScript is null)
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
			DateTime LastPreview = DateTime.Now;
			DateTime TP;

			for (Index = 0; Index < Size; Index++)
			{
				if (Boundary[Index] >= 0 || ColorIndex[Index] >= N)
					DynamicPixels--;
			}

			DateTime Start = DateTime.Now;
			TimeSpan Limit = new TimeSpan(1, 0, 0);

			while (100 * Sum / DynamicPixels > LimitPercentChange && Iterations < 50000 && (DateTime.Now - Start) < Limit)
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

				TP = DateTime.Now;
				if ((TP - LastPreview).TotalSeconds > 5)
				{
					if (!(Node is null))
					{
						LastPreview = TP;

						if (DoPreview)
							Node.Expression.Preview(new GraphBitmap(FractalGraph.ToPixels(ColorIndex, Width, Height, Palette)));

						Node.Expression.Status("Smoothing. Change: " + (100 * Sum / DynamicPixels).ToString("F3") + "%, Limit: " + LimitPercentChange.ToString("F3") + "%, Iterations: " + Iterations.ToString());
					}
				}
			}

			Variables.ConsoleOut.Write("Iterations: " + Iterations.ToString());
			Node.Expression.Status(string.Empty);
		}

		public static void Smooth(double[] R, double[] G, double[] B, double[] A, 
			double[] BoundaryR, double[] BoundaryG, double[] BoundaryB, double[] BoundaryA, 
			int Width, int Height, ScriptNode Node, Variables Variables)
		{
			// Passing color components through the heat equation of 2 spatial dimensions, 
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
			double[] DeltaR = new double[Size];
			double[] DeltaG = new double[Size];
			double[] DeltaB = new double[Size];
			double[] DeltaA = new double[Size];
			double uxx, uyy;
			double d;
			int Iterations = 0;
			int Index;
			int x, y;
			int DynamicPixels = Size;
			double Sum = Size;
			bool DoPreview = Node.Expression.HandlesPreview;
			DateTime LastPreview = DateTime.Now;
			DateTime TP;

			for (Index = 0; Index < Size; Index++)
			{
				if (BoundaryR[Index] >= 0)
					DynamicPixels--;
			}

			DateTime Start = DateTime.Now;
			TimeSpan Limit = new TimeSpan(1, 0, 0);

			while (100 * Sum / DynamicPixels > LimitPercentChange && Iterations < 50000 && (DateTime.Now - Start) < Limit)
			{
				Sum = 0;

				for (y = Index = 0; y < Height; y++)
				{
					for (x = 0; x < Width; x++)
					{
						if (BoundaryR[Index] >= 0 || BoundaryG[Index] >= 0 || BoundaryB[Index] >= 0 || BoundaryA[Index] >= 0)
						{
							DeltaR[Index] = 0;
							DeltaG[Index] = 0;
							DeltaB[Index] = 0;
							DeltaA[Index++] = 0;
							continue;
						}

						d = 2 * R[Index];
						if (x == 0 || x == Width - 1)
							uxx = 0;
						else
							uxx = R[Index - 1] - d + R[Index + 1];

						if (y == 0 || y == Height - 1)
							uyy = 0;
						else
							uyy = R[Index - Width] - d + R[Index + Width];

						d = 0.2 * (uxx + uyy);
						DeltaR[Index] = d;
						Sum += Math.Abs(d);

						d = 2 * G[Index];
						if (x == 0 || x == Width - 1)
							uxx = 0;
						else
							uxx = G[Index - 1] - d + G[Index + 1];

						if (y == 0 || y == Height - 1)
							uyy = 0;
						else
							uyy = G[Index - Width] - d + G[Index + Width];

						d = 0.2 * (uxx + uyy);
						DeltaG[Index] = d;
						Sum += Math.Abs(d);

						d = 2 * B[Index];
						if (x == 0 || x == Width - 1)
							uxx = 0;
						else
							uxx = B[Index - 1] - d + B[Index + 1];

						if (y == 0 || y == Height - 1)
							uyy = 0;
						else
							uyy = B[Index - Width] - d + B[Index + Width];

						d = 0.2 * (uxx + uyy);
						DeltaB[Index] = d;
						Sum += Math.Abs(d);

						d = 2 * A[Index];
						if (x == 0 || x == Width - 1)
							uxx = 0;
						else
							uxx = A[Index - 1] - d + A[Index + 1];

						if (y == 0 || y == Height - 1)
							uyy = 0;
						else
							uyy = A[Index - Width] - d + A[Index + Width];

						d = 0.2 * (uxx + uyy);
						DeltaA[Index] = d;
						Sum += Math.Abs(d);

						Index++;
					}
				}

				for (Index = 0; Index < Size; Index++)
				{
					R[Index] += DeltaR[Index];
					G[Index] += DeltaG[Index];
					B[Index] += DeltaB[Index];
					A[Index] += DeltaA[Index];
				}

				Iterations++;

				TP = DateTime.Now;
				if ((TP - LastPreview).TotalSeconds > 5)
				{
					if (!(Node is null))
					{
						LastPreview = TP;

						if (DoPreview)
							Node.Expression.Preview(new GraphBitmap(FractalGraph.ToPixels(R, G, B, A, Width, Height)));

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
						Boundary[Index] = -1;
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
							Boundary[Index] = -1;
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
							Boundary[Index] = -1;
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
					Boundary[Index] = -1;
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
					Boundary[Index] = -1;
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
						Boundary[Index] = -1;
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
							Boundary[Index] = -1;
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
							Boundary[Index] = -1;
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
					Boundary[Index] = -1;
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

		public static (double[], double[], double[], double[]) FindBoundaries(double[] R, double[] G, double[] B, double[] A, int Width, int Height)
		{
			// Finding boundary values:

			double[] BoundaryR = (double[])R.Clone();
			double[] BoundaryG = (double[])G.Clone();
			double[] BoundaryB = (double[])B.Clone();
			double[] BoundaryA = (double[])A.Clone();
			double dR, dR2;
			double dG, dG2;
			double dB, dB2;
			double dA, dA2;
			int Index;
			int x, y;

			Index = 0;

			dR = R[0];
			dG = G[0];
			dB = B[0];
			dA = A[0];

			dR2 = R[1];
			dG2 = G[1];
			dB2 = B[1];
			dA2 = A[1];

			if (dR <= dR2 && dR > dR2 - 2 &&
				dG <= dG2 && dG > dG2 - 2 &&
				dB <= dB2 && dB > dB2 - 2 &&
				dA <= dA2 && dA > dA2 - 2)
			{
				dR2 = R[Width];
				dG2 = G[Width];
				dB2 = B[Width];
				dA2 = A[Width];

				if (dR <= dR2 && dR > dR2 - 2 &&
					dG <= dG2 && dG > dG2 - 2 &&
					dB <= dB2 && dB > dB2 - 2 &&
					dA <= dA2 && dA > dA2 - 2)
				{
					BoundaryR[0] = -1;
					BoundaryG[0] = -1;
					BoundaryB[0] = -1;
					BoundaryA[0] = -1;
				}
			}

			Index++;

			for (x = 2; x < Width; x++, Index++)
			{
				dR = R[Index];
				dG = G[Index];
				dB = B[Index];
				dA = A[Index];

				dR2 = R[Index + 1];
				dG2 = G[Index + 1];
				dB2 = B[Index + 1];
				dA2 = A[Index + 1];

				if (dR > dR2 || dR <= dR2 - 2 &&
					dG > dG2 || dG <= dG2 - 2 &&
					dB > dB2 || dB <= dB2 - 2 &&
					dA > dA2 || dA <= dA2 - 2)
				{
					continue;
				}

				dR2 = R[Index - 1];
				dG2 = G[Index - 1];
				dB2 = B[Index - 1];
				dA2 = A[Index - 1];

				if (dR > dR2 || dR <= dR2 - 2 &&
					dG > dG2 || dG <= dG2 - 2 &&
					dB > dB2 || dB <= dB2 - 2 &&
					dA > dA2 || dA <= dA2 - 2)
				{
					continue;
				}

				dR2 = R[Index + Width];
				dG2 = G[Index + Width];
				dB2 = B[Index + Width];
				dA2 = A[Index + Width];

				if (dR > dR2 || dR <= dR2 - 2 &&
					dG > dG2 || dG <= dG2 - 2 &&
					dB > dB2 || dB <= dB2 - 2 &&
					dA > dA2 || dA <= dA2 - 2)
				{
					continue;
				}

				BoundaryR[Index] = -1;
				BoundaryG[Index] = -1;
				BoundaryB[Index] = -1;
				BoundaryA[Index] = -1;
			}

			dR2 = R[Index];
			dG2 = G[Index];
			dB2 = B[Index];
			dA2 = A[Index];

			if (dR <= dR2 && dR > dR2 - 2 &&
				dG <= dG2 && dG > dG2 - 2 &&
				dB <= dB2 && dB > dB2 - 2 &&
				dA <= dA2 && dA > dA2 - 2)
			{
				dR2 = R[Index - 1];
				dG2 = G[Index - 1];
				dB2 = B[Index - 1];
				dA2 = A[Index - 1];

				if (dR <= dR2 && dR > dR2 - 2 &&
					dG <= dG2 && dG > dG2 - 2 &&
					dB <= dB2 && dB > dB2 - 2 &&
					dA <= dA2 && dA > dA2 - 2)
				{
					dR2 = R[Index + Width];
					dG2 = G[Index + Width];
					dB2 = B[Index + Width];
					dA2 = A[Index + Width];

					if (dR <= dR2 && dR > dR2 - 2 &&
						dG <= dG2 && dG > dG2 - 2 &&
						dB <= dB2 && dB > dB2 - 2 &&
						dA <= dA2 && dA > dA2 - 2)
					{
						BoundaryR[Index] = -1;
						BoundaryG[Index] = -1;
						BoundaryB[Index] = -1;
						BoundaryA[Index] = -1;
					}
				}
			}

			Index++;

			for (y = 2; y < Height; y++)
			{
				dR = R[Index];
				dG = G[Index];
				dB = B[Index];
				dA = A[Index];

				dR2 = R[Index + 1];
				dG2 = G[Index + 1];
				dB2 = B[Index + 1];
				dA2 = A[Index + 1];

				if (dR <= dR2 && dR > dR2 - 2 &&
					dG <= dG2 && dG > dG2 - 2 &&
					dB <= dB2 && dB > dB2 - 2 &&
					dA <= dA2 && dA > dA2 - 2)
				{
					dR2 = R[Index - Width];
					dG2 = G[Index - Width];
					dB2 = B[Index - Width];
					dA2 = A[Index - Width];

					if (dR <= dR2 && dR > dR2 - 2 &&
						dG <= dG2 && dG > dG2 - 2 &&
						dB <= dB2 && dB > dB2 - 2 &&
						dA <= dA2 && dA > dA2 - 2)
					{
						dR2 = R[Index + Width];
						dG2 = G[Index + Width];
						dB2 = B[Index + Width];
						dA2 = A[Index + Width];

						if (dR <= dR2 && dR > dR2 - 2 &&
							dG <= dG2 && dG > dG2 - 2 &&
							dB <= dB2 && dB > dB2 - 2 &&
							dA <= dA2 && dA > dA2 - 2)
						{
							BoundaryR[Index] = -1;
							BoundaryG[Index] = -1;
							BoundaryB[Index] = -1;
							BoundaryA[Index] = -1;
						}
					}
				}

				Index++;

				for (x = 2; x < Width; x++, Index++)
				{
					dR = R[Index];
					dG = G[Index];
					dB = B[Index];
					dA = A[Index];

					dR2 = R[Index + 1];
					dG2 = G[Index + 1];
					dB2 = B[Index + 1];
					dA2 = A[Index + 1];

					if (dR > dR2 || dR <= dR2 - 2 &&
						dG > dG2 || dG <= dG2 - 2 &&
						dB > dB2 || dB <= dB2 - 2 &&
						dA > dA2 || dA <= dA2 - 2)
					{
						continue;
					}

					dR2 = R[Index - 1];
					dG2 = G[Index - 1];
					dB2 = B[Index - 1];
					dA2 = A[Index - 1];

					if (dR > dR2 || dR <= dR2 - 2 &&
						dG > dG2 || dG <= dG2 - 2 &&
						dB > dB2 || dB <= dB2 - 2 &&
						dA > dA2 || dA <= dA2 - 2)
					{
						continue;
					}

					dR2 = R[Index + Width];
					dG2 = G[Index + Width];
					dB2 = B[Index + Width];
					dA2 = A[Index + Width];

					if (dR > dR2 || dR <= dR2 - 2 &&
						dG > dG2 || dG <= dG2 - 2 &&
						dB > dB2 || dB <= dB2 - 2 &&
						dA > dA2 || dA <= dA2 - 2)
					{
						continue;
					}

					dR2 = R[Index - Width];
					dG2 = G[Index - Width];
					dB2 = B[Index - Width];
					dA2 = A[Index - Width];

					if (dR > dR2 || dR <= dR2 - 2 &&
						dG > dG2 || dG <= dG2 - 2 &&
						dB > dB2 || dB <= dB2 - 2 &&
						dA > dA2 || dA <= dA2 - 2)
					{
						continue;
					}

					BoundaryR[Index] = -1;
					BoundaryG[Index] = -1;
					BoundaryB[Index] = -1;
					BoundaryA[Index] = -1;
				}

				dR = R[Index];
				dG = G[Index];
				dB = B[Index];
				dA = A[Index];

				dR2 = R[Index - 1];
				dG2 = G[Index - 1];
				dB2 = B[Index - 1];
				dA2 = A[Index - 1];

				if (dR <= dR2 && dR > dR2 - 2 &&
					dG <= dG2 && dG > dG2 - 2 &&
					dB <= dB2 && dB > dB2 - 2 &&
					dA <= dA2 && dA > dA2 - 2)
				{
					dR2 = R[Index - Width];
					dG2 = G[Index - Width];
					dB2 = B[Index - Width];
					dA2 = A[Index - Width];

					if (dR <= dR2 && dR > dR2 - 2 &&
						dG <= dG2 && dG > dG2 - 2 &&
						dB <= dB2 && dB > dB2 - 2 &&
						dA <= dA2 && dA > dA2 - 2)
					{
						dR2 = R[Index + Width];
						dG2 = G[Index + Width];
						dB2 = B[Index + Width];
						dA2 = A[Index + Width];

						if (dR <= dR2 && dR > dR2 - 2 &&
							dG <= dG2 && dG > dG2 - 2 &&
							dB <= dB2 && dB > dB2 - 2 &&
							dA <= dA2 && dA > dA2 - 2)
						{
							BoundaryR[Index] = -1;
							BoundaryG[Index] = -1;
							BoundaryB[Index] = -1;
							BoundaryA[Index] = -1;
						}
					}
				}

				Index++;
			}

			dR = R[Index];
			dG = G[Index];
			dB = B[Index];
			dA = A[Index];

			dR2 = R[Index + 1];
			dG2 = G[Index + 1];
			dB2 = B[Index + 1];
			dA2 = A[Index + 1];

			if (dR <= dR2 && dR > dR2 - 2 &&
				dG <= dG2 && dG > dG2 - 2 &&
				dB <= dB2 && dB > dB2 - 2 &&
				dA <= dA2 && dA > dA2 - 2)
			{
				dR2 = R[Index - Width];
				dG2 = G[Index - Width];
				dB2 = B[Index - Width];
				dA2 = A[Index - Width];

				if (dR <= dR2 && dR > dR2 - 2 &&
					dG <= dG2 && dG > dG2 - 2 &&
					dB <= dB2 && dB > dB2 - 2 &&
					dA <= dA2 && dA > dA2 - 2)
				{
					BoundaryR[Index] = -1;
					BoundaryG[Index] = -1;
					BoundaryB[Index] = -1;
					BoundaryA[Index] = -1;
				}
			}

			Index++;

			for (x = 2; x < Width; x++, Index++)
			{
				dR = R[Index];
				dG = G[Index];
				dB = B[Index];
				dA = A[Index];

				dR2 = R[Index + 1];
				dG2 = G[Index + 1];
				dB2 = B[Index + 1];
				dA2 = A[Index + 1];

				if (dR > dR2 || dR <= dR2 - 2 &&
					dG > dG2 || dG <= dG2 - 2 &&
					dB > dB2 || dB <= dB2 - 2 &&
					dA > dA2 || dA <= dA2 - 2)
				{
					continue;
				}

				dR2 = R[Index - 1];
				dG2 = G[Index - 1];
				dB2 = B[Index - 1];
				dA2 = A[Index - 1];

				if (dR > dR2 || dR <= dR2 - 2 &&
					dG > dG2 || dG <= dG2 - 2 &&
					dB > dB2 || dB <= dB2 - 2 &&
					dA > dA2 || dA <= dA2 - 2)
				{
					continue;
				}

				dR2 = R[Index - Width];
				dG2 = G[Index - Width];
				dB2 = B[Index - Width];
				dA2 = A[Index - Width];

				if (dR > dR2 || dR <= dR2 - 2 &&
					dG > dG2 || dG <= dG2 - 2 &&
					dB > dB2 || dB <= dB2 - 2 &&
					dA > dA2 || dA <= dA2 - 2)
				{
					continue;
				}

				BoundaryR[Index] = -1;
				BoundaryG[Index] = -1;
				BoundaryB[Index] = -1;
				BoundaryA[Index] = -1;
			}

			dR = R[Index];
			dG = G[Index];
			dB = B[Index];
			dA = A[Index];

			dR2 = R[Index - 1];
			dG2 = G[Index - 1];
			dB2 = B[Index - 1];
			dA2 = A[Index - 1];

			if (dR <= dR2 && dR > dR2 - 2 &&
				dG <= dG2 && dG > dG2 - 2 &&
				dB <= dB2 && dB > dB2 - 2 &&
				dA <= dA2 && dA > dA2 - 2)
			{
				dR2 = R[Index - Width];
				dG2 = G[Index - Width];
				dB2 = B[Index - Width];
				dA2 = A[Index - Width];

				if (dR <= dR2 && dR > dR2 - 2 &&
					dG <= dG2 && dG > dG2 - 2 &&
					dB <= dB2 && dB > dB2 - 2 &&
					dA <= dA2 && dA > dA2 - 2)
				{
					BoundaryR[Index] = -1;
					BoundaryG[Index] = -1;
					BoundaryB[Index] = -1;
					BoundaryA[Index] = -1;
				}
			}

			return (BoundaryR, BoundaryG, BoundaryB, BoundaryA);
		}

		public static PixelInformation ToPixels(double[] ColorIndex, int Width, int Height, SKColor[] Palette)
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

			return PixelInformation.FromRaw(SKColorType.Bgra8888, rgb, Width, Height, Width << 2);
		}

		public static PixelInformation ToPixels(int[] ColorIndex, int Width, int Height, SKColor[] Palette)
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

			return PixelInformation.FromRaw(SKColorType.Bgra8888, rgb, Width, Height, Width << 2);
		}

		public static PixelInformation ToPixels(double[] R, double[] G, double[] B, double[] A, int Width, int Height)
		{
			int Size = Width * Height;
			int Size4 = Size * 4;
			byte[] rgb = new byte[Size4];
			int Index2;
			int ci;
			int Index;

			for (Index = Index2 = 0; Index < Size; Index++)
			{
				ci = (int)(B[Index] + 0.5);
				rgb[Index2++] = (byte)(ci < 0 ? 0 : ci > 255 ? 255 : ci);

				ci = (int)(G[Index] + 0.5);
				rgb[Index2++] = (byte)(ci < 0 ? 0 : ci > 255 ? 255 : ci);

				ci = (int)(R[Index] + 0.5);
				rgb[Index2++] = (byte)(ci < 0 ? 0 : ci > 255 ? 255 : ci);

				ci = (int)(A[Index] + 0.5);
				rgb[Index2++] = (byte)(ci < 0 ? 0 : ci > 255 ? 255 : ci);
			}

			return PixelInformation.FromRaw(SKColorType.Bgra8888, rgb, Width, Height, Width << 2);
		}

	}
}
