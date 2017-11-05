using System;
using System.Numerics;
using SkiaSharp;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// 3D drawing area.
	/// </summary>
	public class Canvas3D
	{
		private uint[] colors;
		private float[] zBuffer;
		private Matrix4x4 t;
		private Vector4 last = Vector4.Zero;
		private int width;
		private int height;
		private int overSampling;
		private int w;
		private int h;
		private int wm1;
		private int hm1;
		private int cx;
		private int cy;

		/// <summary>
		/// 3D drawing area.
		/// 
		/// By default, the camera is looking along the z-axis, with no projection, and no scaling.
		/// The center of the canvas is located at origo.
		/// </summary>
		/// <param name="Width">Width of area, in pixels.</param>
		/// <param name="Height">Height of area, in pixels.</param>
		/// <param name="Oversampling">Number of subpixels for each generated pixel.
		/// Oversampling provides a means to achieve anti-aliasing in the rendered result.</param>
		/// <param name="BackgroundColor">Background color</param>
		public Canvas3D(int Width, int Height, int Oversampling, SKColor BackgroundColor)
		{
			if (Width <= 0)
				throw new ArgumentException("Width must be a positive integer.", nameof(Width));

			if (Height <= 0)
				throw new ArgumentException("Height must be a positive integer.", nameof(Height));

			if (Oversampling <= 0)
				throw new ArgumentException("Oversampling must be a positive integer.", nameof(Oversampling));

			this.width = Width;
			this.height = Height;
			this.overSampling = Oversampling;
			this.w = Width * Oversampling;
			this.h = Height * Oversampling;
			this.wm1 = this.w - 1;
			this.hm1 = this.h - 1;
			this.cx = this.w / 2;
			this.cy = this.h / 2;
			this.ResetTransforms();

			int i, c = this.w * this.h;
			uint BgColor = ToUInt(BackgroundColor);

			this.colors = new uint[c];
			this.zBuffer = new float[c];

			for (i = 0; i < c; i++)
			{
				this.colors[i] = BgColor;
				this.zBuffer[i] = float.MaxValue;
			}
		}

		private static uint ToUInt(SKColor Color)
		{
			uint Result = Color.Alpha;
			Result <<= 8;
			Result |= Color.Blue;
			Result <<= 8;
			Result |= Color.Green;
			Result <<= 8;
			Result |= Color.Red;

			return Result;
		}

		#region Bitmaps

		/// <summary>
		/// Creates a bitmap from the pixels in the canvas.
		/// </summary>
		/// <returns></returns>
		public SKImage GetBitmap()
		{
			if (this.overSampling == 1)
				return this.GetBitmap(this.colors);
			else
			{
				uint[] Colors = new uint[this.width * this.height];
				int x, y, dx, dy, p0, p, q = 0;
				int o2 = this.overSampling * this.overSampling;
				int h = o2 >> 1;
				uint SumR, SumG, SumB, SumA;
				uint Color;

				for (y = 0; y < this.height; y++)
				{
					for (x = 0; x < this.width; x++)
					{
						SumR = SumG = SumB = SumA = 0;
						p0 = ((y * this.w) + x) * this.overSampling;

						for (dy = 0; dy < this.overSampling; dy++, p0 += this.w)
						{
							for (dx = 0, p = p0; dx < this.overSampling; dx++, p++)
							{
								Color = this.colors[p];
								SumR += (byte)Color;
								Color >>= 8;
								SumG += (byte)Color;
								Color >>= 8;
								SumB += (byte)Color;
								Color >>= 8;
								SumA += (byte)Color;
							}
						}

						Color = (byte)((SumA + h) / o2);
						Color <<= 8;
						Color |= (byte)((SumB + h) / o2);
						Color <<= 8;
						Color |= (byte)((SumG + h) / o2);
						Color <<= 8;
						Color |= (byte)((SumR + h) / o2);

						Colors[q++] = Color;
					}
				}

				return this.GetBitmap(Colors);
			}
		}

		private SKImage GetBitmap(uint[] Colors)
		{
			int c = Colors.Length << 2;
			byte[] ByteArray = new byte[c];

			if (BitConverter.IsLittleEndian)
				Buffer.BlockCopy(Colors, 0, ByteArray, 0, c);
			else
			{
				int d = Colors.Length;
				int i, k = 0;
				uint j;

				for (i = 0; i < d; i++)
				{
					j = Colors[i];

					ByteArray[k++] = (byte)j;
					j >>= 8;
					ByteArray[k++] = (byte)j;
					j >>= 8;
					ByteArray[k++] = (byte)j;
					j >>= 8;
					ByteArray[k++] = (byte)j;
				}
			}

			using (SKData Data = SKData.CreateCopy(ByteArray))
			{
				SKImageInfo ImageInfo = new SKImageInfo(this.width, this.height, SKColorType.Rgba8888, SKAlphaType.Premul);
				return SKImage.FromPixelData(ImageInfo, Data, this.width << 2);
			}
		}

		#endregion

		#region Transforms

		/// <summary>
		/// Resets any transforms.
		/// </summary>
		public void ResetTransforms()
		{
			this.t = new Matrix4x4(this.overSampling, 0, 0, 0, 0, this.overSampling, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
		}

		/// <summary>
		/// Multiplies a projection matrix (along the Z-axis) to the current transformation.
		/// </summary>
		/// <param name="Distance">Distance between projection plane and camera.</param>
		public void ProjectZ(float Distance)
		{
			Matrix4x4 M = new Matrix4x4(
				Distance, 0, 0, 0,
				0, Distance, 0, 0,
				0, 0, Distance, 1,
				0, 0, 0, Distance);

			this.t = M * this.t;
		}

		#endregion

		#region Plot

		/// <summary>
		/// Plots a point on the 3D-canvas.
		/// </summary>
		/// <param name="Point">Point to plot.</param>
		/// <param name="Color">Color.</param>
		public void Plot(Vector4 Point, SKColor Color)
		{
			this.Plot(Point, ToUInt(Color));
		}

		/// <summary>
		/// Plots a point on the 3D-canvas.
		/// </summary>
		/// <param name="Point">Point to plot.</param>
		/// <param name="Color">Color.</param>
		public void Plot(Vector4 Point, uint Color)
		{
			this.last = Point;

			Point = Vector4.Transform(Point, this.t);
			int x = this.cx + (int)(Point.X / Point.W + 0.5f);
			int y = this.cy - (int)(Point.Y / Point.W + 0.5f);

			if (x >= 0 && x < this.w && y >= 0 && y < this.h)
				this.Plot(x, y, Point.Z, Color);
		}

		private void Plot(int x, int y, float z, uint Color)
		{
			int p = y * this.w + x;

			if (z >= 0 && z < this.zBuffer[p])
			{
				this.zBuffer[p] = z;

				byte A = (byte)(Color >> 24);
				if (A == 255)
					this.colors[p] = Color;
				else
				{
					uint Color2 = this.colors[p];
					byte R = (byte)Color;
					byte G = (byte)(Color >> 8);
					byte B = (byte)(Color >> 16);
					byte A2 = (byte)(Color2 >> 24);
					byte R2 = (byte)Color2;
					byte G2 = (byte)(Color2 >> 8);
					byte B2 = (byte)(Color2 >> 16);
					byte R3, G3, B3, A3;

					if (A2 == 255)
					{
						R3 = (byte)(((R * A + R2 * (255 - A)) + 128) / 255);
						G3 = (byte)(((G * A + G2 * (255 - A)) + 128) / 255);
						B3 = (byte)(((B * A + B2 * (255 - A)) + 128) / 255);
						A3 = 255;
					}
					else
					{
						R2 = (byte)((R2 * A2 + 128) / 255);
						G2 = (byte)((G2 * A2 + 128) / 255);
						B2 = (byte)((B2 * A2 + 128) / 255);

						R3 = (byte)(((R * A + R2 * (255 - A)) + 128) / 255);
						G3 = (byte)(((G * A + G2 * (255 - A)) + 128) / 255);
						B3 = (byte)(((B * A + B2 * (255 - A)) + 128) / 255);
						A3 = (byte)(255 - (((255 - A) * (255 - A2) + 128) / 255));
					}

					this.colors[p] = (uint)(((((A3 << 8) | B3) << 8) | G3) << 8) | R3;
				}
			}
		}

		#endregion

		#region Lines

		private bool ClipLine(ref float x0, ref float y0, ref float z0, ref float x1, ref float y1, ref float z1)
		{
			byte Mask0 = 0;
			byte Mask1 = 0;
			float Delta;
			float Delta2;

			if (x0 < 0)
				Mask0 |= 1;
			else if (x0 > this.wm1)
				Mask0 |= 2;

			if (y0 < 0)
				Mask0 |= 4;
			else if (y0 > this.hm1)
				Mask0 |= 8;

			if (x1 < 0)
				Mask1 |= 1;
			else if (x1 > this.wm1)
				Mask1 |= 2;

			if (y1 < 0)
				Mask1 |= 4;
			else if (y1 > this.hm1)
				Mask1 |= 8;

			if (Mask0 == 0 && Mask1 == 0)
				return true;

			if ((Mask0 & Mask1) != 0)
				return false;

			// Left edge:

			if ((Mask0 & 1) != 0)
			{
				Delta = x1 - x0;    // Must be non-zero, or masks would have common bit.
				y0 = y0 - x0 * (y1 - y0) / Delta;
				z0 = z0 - x0 * (z1 - z0) / Delta;
				x0 = 0;

				Mask0 &= 254;
				if (y0 < 0)
					Mask0 |= 4;
				else if (y0 > this.hm1)
					Mask0 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 1) != 0)
			{
				Delta = x0 - x1;    // Must be non-zero, or masks would have common bit.
				y1 = y1 - x1 * (y0 - y1) / Delta;
				z1 = z1 - x1 * (z0 - z1) / Delta;
				x1 = 0;

				Mask1 &= 254;
				if (y1 < 0)
					Mask1 |= 4;
				else if (y1 > this.hm1)
					Mask1 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			// Top edge:

			if ((Mask0 & 4) != 0)
			{
				Delta = y1 - y0;    // Must be non-zero, or masks would have common bit.
				x0 = x0 - y0 * (x1 - x0) / Delta;
				z0 = z0 - y0 * (z1 - z0) / Delta;
				y0 = 0;

				Mask0 &= 251;
				if (x0 < 0)
					Mask0 |= 1;
				else if (x0 > this.wm1)
					Mask0 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 4) != 0)
			{
				Delta = y0 - y1;    // Must be non-zero, or masks would have common bit.
				x1 = x1 - y1 * (x0 - x1) / Delta;
				z1 = z1 - y1 * (z0 - z1) / Delta;
				y1 = 0;

				Mask1 &= 251;
				if (x1 < 0)
					Mask1 |= 1;
				else if (x1 > this.wm1)
					Mask1 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			// Right edge:

			if ((Mask0 & 2) != 0)
			{
				Delta = x1 - x0;    // Must be non-zero, or masks would have common bit.
				Delta2 = this.wm1 - x0;
				y0 = y0 + Delta2 * (y1 - y0) / Delta;
				z0 = z0 + Delta2 * (z1 - z0) / Delta;
				x0 = this.wm1;

				Mask0 &= 253;
				if (y0 < 0)
					Mask0 |= 4;
				else if (y0 > this.hm1)
					Mask0 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 2) != 0)
			{
				Delta = x0 - x1;    // Must be non-zero, or masks would have common bit.
				Delta2 = this.wm1 - x1;
				y1 = y1 + Delta2 * (y0 - y1) / Delta;
				z1 = z1 + Delta2 * (z0 - z1) / Delta;
				x1 = this.wm1;

				Mask1 &= 253;
				if (y1 < 0)
					Mask1 |= 4;
				else if (y1 > this.hm1)
					Mask1 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			// Bottom edge:

			if ((Mask0 & 8) != 0)
			{
				Delta = y1 - y0;    // Must be non-zero, or masks would have common bit.
				Delta2 = this.hm1 - y0;
				x0 = x0 + Delta2 * (x1 - x0) / Delta;
				z0 = z0 + Delta2 * (z1 - z0) / Delta;
				y0 = this.hm1;

				Mask0 &= 247;
				if (x0 < 0)
					Mask0 |= 1;
				else if (x0 > this.wm1)
					Mask0 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 8) != 0)
			{
				Delta = y0 - y1;    // Must be non-zero, or masks would have common bit.
				Delta2 = this.hm1 - y1;
				x1 = x1 + Delta2 * (x0 - x1) / Delta;
				z1 = z1 + Delta2 * (z0 - z1) / Delta;
				y1 = this.hm1;

				Mask1 &= 247;
				if (x1 < 0)
					Mask1 |= 1;
				else if (x1 > this.wm1)
					Mask1 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			return ((Mask0 | Mask1) == 0);
		}

		/// <summary>
		/// Draws a line between P0 and P1.
		/// </summary>
		/// <param name="P0">Point 1.</param>
		/// <param name="P1">Point 2.</param>
		/// <param name="Color">Color</param>
		public void Line(Vector4 P0, Vector4 P1, SKColor Color)
		{
			this.Line(P0, P1, ToUInt(Color));
		}

		/// <summary>
		/// Draws a line between P0 and P1.
		/// </summary>
		/// <param name="P0">Point 1.</param>
		/// <param name="P1">Point 2.</param>
		/// <param name="Color">Color</param>
		public void Line(Vector4 P0, Vector4 P1, uint Color)
		{
			this.last = P1;

			P0 = Vector4.Transform(P0, this.t);
			P1 = Vector4.Transform(P1, this.t);
			float x0 = this.cx + P0.X / P0.W;
			float y0 = this.cy - P0.Y / P0.W;
			float z0 = P0.Z;
			float x1 = this.cx + P1.X / P1.W;
			float y1 = this.cy - P1.Y / P1.W;
			float z1 = P1.Z;

			if (this.ClipLine(ref x0, ref y0, ref z0, ref x1, ref y1, ref z1))
			{
				float dx = x1 - x0;
				float dy = y1 - y0;
				float dz;
				float temp;

				this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0, Color);

				if (Math.Abs(dy) >= Math.Abs(dx))
				{
					if (dy == 0)
						return;

					if (dy < 0)
					{
						temp = x0;
						x0 = x1;
						x1 = temp;

						temp = y0;
						y0 = y1;
						y1 = temp;

						temp = z0;
						z0 = z1;
						z1 = temp;

						dx = -dx;
						dy = -dy;
					}

					dz = (z1 - z0) / dy;
					dx /= dy;

					temp = 1 - (y0 - ((int)y0));
					y0 += temp;
					x0 += dx * temp;
					z0 += dz * temp;

					while (y0 <= y1)
					{
						this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0, Color);
						y0++;
						x0 += dx;
						z0 += dz;
					}

					temp = y1 - ((int)y1);
					if (temp > 0)
					{
						temp = 1 - temp;

						y0 -= temp;
						x0 -= dx * temp;
						z0 -= dz * temp;

						this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0, Color);
					}
				}
				else
				{
					if (dx < 0)
					{
						temp = x0;
						x0 = x1;
						x1 = temp;

						temp = y0;
						y0 = y1;
						y1 = temp;

						temp = z0;
						z0 = z1;
						z1 = temp;

						dx = -dx;
						dy = -dy;
					}

					dz = (z1 - z0) / dx;
					dy /= dx;

					temp = 1 - (x0 - ((int)x0));
					x0 += temp;
					y0 += dy * temp;
					z0 += dz * temp;

					while (x0 <= x1)
					{
						this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0, Color);
						x0++;
						y0 += dy;
						z0 += dz;
					}

					temp = x1 - ((int)x1);
					if (temp > 0)
					{
						temp = 1 - temp;

						x0 -= temp;
						y0 -= dy * temp;
						z0 -= dz * temp;

						this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0, Color);
					}
				}
			}
		}

		/// <summary>
		/// Moves to a point.
		/// </summary>
		/// <param name="Point">Point.</param>
		public void MoveTo(Vector4 Point)
		{
			this.last = Point;
		}

		/// <summary>
		/// Draws a line to <paramref name="Point"/> from the last endpoint.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <param name="Color">Color</param>
		public void LineTo(Vector4 Point, SKColor Color)
		{
			this.LineTo(Point, ToUInt(Color));
		}

		/// <summary>
		/// Draws a line to <paramref name="Point"/> from the last endpoint.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <param name="Color">Color</param>
		public void LineTo(Vector4 Point, uint Color)
		{
			this.Line(this.last, Point, Color);
		}

		/// <summary>
		/// Draws lines between a set of nodes.
		/// </summary>
		/// <param name="Nodes">Nodes</param>
		/// <param name="Color">Color</param>
		public void PolyLine(Vector4[] Nodes, SKColor Color)
		{
			this.PolyLine(Nodes, ToUInt(Color));
		}

		/// <summary>
		/// Draws lines between a set of nodes.
		/// </summary>
		/// <param name="Nodes">Nodes</param>
		/// <param name="Color">Color</param>
		public void PolyLine(Vector4[] Nodes, uint Color)
		{
			int i, c = Nodes.Length;

			this.MoveTo(Nodes[0]);

			for (i = 1; i < c; i++)
				this.LineTo(Nodes[i], Color);
		}

		#endregion

	}
}
