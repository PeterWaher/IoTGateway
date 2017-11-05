using System;
using System.Collections.Generic;
using System.Numerics;
using SkiaSharp;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// 3D drawing area.
	/// </summary>
	public class Canvas3D
	{
		private byte[] pixels;
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

			int i, j, c = this.w * this.h;
			byte R = BackgroundColor.Red;
			byte G = BackgroundColor.Green;
			byte B = BackgroundColor.Blue;
			byte A = BackgroundColor.Alpha;

			this.pixels = new byte[c * 4];
			this.zBuffer = new float[c];

			for (i = j = 0; i < c; i++)
			{
				this.pixels[j++] = R;
				this.pixels[j++] = G;
				this.pixels[j++] = B;
				this.pixels[j++] = A;

				this.zBuffer[i] = float.MaxValue;
			}
		}

		#region Colors

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

		private static SKColor ToColor(uint Color)
		{
			byte R = (byte)Color;
			Color >>= 8;
			byte G = (byte)Color;
			Color >>= 8;
			byte B = (byte)Color;
			Color >>= 8;
			byte A = (byte)Color;

			return new SKColor(R, G, B, A);
		}

		#endregion

		#region Bitmaps

		/// <summary>
		/// Creates a bitmap from the pixels in the canvas.
		/// </summary>
		/// <returns></returns>
		public SKImage GetBitmap()
		{
			if (this.overSampling == 1)
				return this.GetBitmap(this.pixels);
			else
			{
				byte[] Pixels = new byte[this.width * this.height * 4];
				int x, y, dx, dy, p0, p, q = 0;
				int o2 = this.overSampling * this.overSampling;
				int h = o2 >> 1;
				uint SumR, SumG, SumB, SumA;

				for (y = 0; y < this.height; y++)
				{
					for (x = 0; x < this.width; x++)
					{
						SumR = SumG = SumB = SumA = 0;
						p0 = ((y * this.w) + x) * this.overSampling * 4;

						for (dy = 0; dy < this.overSampling; dy++, p0 += this.w * 4)
						{
							for (dx = 0, p = p0; dx < this.overSampling; dx++)
							{
								SumR += this.pixels[p++];
								SumG += this.pixels[p++];
								SumB += this.pixels[p++];
								SumA += this.pixels[p++];
							}
						}

						Pixels[q++] = (byte)((SumR + h) / o2);
						Pixels[q++] = (byte)((SumG + h) / o2);
						Pixels[q++] = (byte)((SumB + h) / o2);
						Pixels[q++] = (byte)((SumA + h) / o2);
					}
				}

				return this.GetBitmap(Pixels);
			}
		}

		private SKImage GetBitmap(byte[] Pixels)
		{
			using (SKData Data = SKData.CreateCopy(Pixels))
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

				p <<= 2;

				byte A = (byte)(Color >> 24);
				if (A == 255)
				{
					this.pixels[p++] = (byte)Color;
					Color >>= 8;
					this.pixels[p++] = (byte)Color;
					Color >>= 8;
					this.pixels[p++] = (byte)Color;
					Color >>= 8;
					this.pixels[p] = (byte)Color;
				}
				else
				{
					byte R = (byte)Color;
					byte G = (byte)(Color >> 8);
					byte B = (byte)(Color >> 16);
					byte R2 = this.pixels[p++];
					byte G2 = this.pixels[p++];
					byte B2 = this.pixels[p++];
					byte A2 = this.pixels[p];
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

					this.pixels[p--] = A3;
					this.pixels[p--] = B3;
					this.pixels[p--] = G3;
					this.pixels[p] = R3;
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
				y0 -= x0 * (y1 - y0) / Delta;
				z0 -= x0 * (z1 - z0) / Delta;
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
				y1 -= x1 * (y0 - y1) / Delta;
				z1 -= x1 * (z0 - z1) / Delta;
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
				x0 -= y0 * (x1 - x0) / Delta;
				z0 -= y0 * (z1 - z0) / Delta;
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
				x1 -= y1 * (x0 - x1) / Delta;
				z1 -= y1 * (z0 - z1) / Delta;
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
				y0 += Delta2 * (y1 - y0) / Delta;
				z0 += Delta2 * (z1 - z0) / Delta;
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
				y1 += Delta2 * (y0 - y1) / Delta;
				z1 += Delta2 * (z0 - z1) / Delta;
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
				x0 += Delta2 * (x1 - x0) / Delta;
				z0 += Delta2 * (z1 - z0) / Delta;
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
				x1 += Delta2 * (x0 - x1) / Delta;
				z1 += Delta2 * (z0 - z1) / Delta;
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

		#region Scan Lines

		private void ScanLine(float x0, float y0, float z0, SKColor Color0, float x1, float z1, SKColor Color1)
		{
			float Delta, Delta2;

			if (x1 < x0)
			{
				Delta = x0;
				x0 = x1;
				x1 = Delta;

				Delta = z0;
				z0 = z1;
				z1 = Delta;

				SKColor cl = Color0;
				Color0 = Color1;
				Color1 = cl;
			}

			float y1 = y0;
			float x0b = x0;
			float x1b = x1;

			if (!this.ClipLine(ref x0, ref y0, ref z0, ref x1, ref y1, ref z1))
				return;

			if (x0 == x1)
			{
				if (z0 < z1)
					this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0, ToUInt(Color0));
				else
					this.Plot((int)(x1 + 0.5f), (int)(y0 + 0.5f), z1, ToUInt(Color1));
			}
			else
			{
				float R0 = Color0.Red;
				float G0 = Color0.Green;
				float B0 = Color0.Blue;
				float A0 = Color0.Alpha;
				float R1 = Color1.Red;
				float G1 = Color1.Green;
				float B1 = Color1.Blue;
				float A1 = Color1.Alpha;

				if (x0 != x0b)
				{
					Delta = x0 - x0b;
					Delta2 = x1b - x0b;

					R0 += Delta * (R1 - R0) / Delta2;
					G0 += Delta * (G1 - G0) / Delta2;
					B0 += Delta * (B1 - B0) / Delta2;
					A0 += Delta * (A1 - A0) / Delta2;
				}

				if (x1 != x1b)
				{
					Delta = x1 - x1b;
					Delta2 = x0b - x1b;

					R1 += Delta * (R0 - R1) / Delta2;
					G1 += Delta * (G0 - G1) / Delta2;
					B1 += Delta * (B0 - B1) / Delta2;
					A1 += Delta * (A0 - A1) / Delta2;
				}

				Delta = x0 - (int)x0;
				if (Delta > 0)
				{
					Delta2 = x1 - x0;
					x0 -= Delta;
					z0 -= Delta * (z1 - z0) / Delta2;
					R0 -= Delta * (R1 - R0) / Delta2;
					G0 -= Delta * (G1 - G0) / Delta2;
					B0 -= Delta * (B1 - B0) / Delta2;
					A0 -= Delta * (A1 - A0) / Delta2;
				}

				int p = (int)(y0 + 0.5f) * this.w + (int)x0;
				int p4 = p << 2;
				float dx = (x1 - x0);
				float dz = (z1 - z0) / dx;
				float dR = (R1 - R0) / dx;
				float dG = (G1 - G0) / dx;
				float dB = (B1 - B0) / dx;
				float dA = (A1 - A0) / dx;

				while (x0 <= x1)
				{
					if (z0 > 0 && z0 < this.zBuffer[p])
					{
						this.zBuffer[p++] = z0;

						if (A0 == 255)
						{
							this.pixels[p4++] = (byte)(R0 + 0.5f);
							this.pixels[p4++] = (byte)(G0 + 0.5f);
							this.pixels[p4++] = (byte)(B0 + 0.5f);
							this.pixels[p4++] = (byte)(A0 + 0.5f);
						}
						else
						{
							byte R = (byte)(R0 + 0.5f);
							byte G = (byte)(G0 + 0.5f);
							byte B = (byte)(B0 + 0.5f);
							byte A = (byte)(A0 + 0.5f);
							byte R2 = this.pixels[p4++];
							byte G2 = this.pixels[p4++];
							byte B2 = this.pixels[p4++];
							byte A2 = this.pixels[p4];
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

							this.pixels[p4--] = A3;
							this.pixels[p4--] = B3;
							this.pixels[p4--] = G3;
							this.pixels[p4] = R3;
							p4 += 4;
						}
					}
					else
					{
						p++;
						p4 += 4;
					}

					x0++;
					z0 += dz;
					R0 += dR;
					G0 += dG;
					B0 += dB;
					A0 += dA;
				}
			}
		}

		#endregion

		#region Polygon

		private static Vector3 ToVector3(Vector4 P)
		{
			float w = 1.0f / P.W;
			return new Vector3(P.X * w, P.Y * w, P.Z * w);
		}

		private static Vector3 CalcNormal(Vector3 P0, Vector3 P1, Vector3 P2)
		{
			return Vector3.Normalize(Vector3.Cross(P1 - P0, P2 - P0));
		}

		private bool ClipTopBottom(ref float x0, ref float y0, ref float z0, ref float x1, ref float y1, ref float z1)
		{
			byte Mask0 = 0;
			byte Mask1 = 0;
			float Delta;
			float Delta2;

			if (y0 < 0)
				Mask0 |= 4;
			else if (y0 > this.hm1)
				Mask0 |= 8;

			if (y1 < 0)
				Mask1 |= 4;
			else if (y1 > this.hm1)
				Mask1 |= 8;

			if (Mask0 == 0 && Mask1 == 0)
				return true;

			if ((Mask0 & Mask1) != 0)
				return false;

			// Top edge:

			if ((Mask0 & 4) != 0)
			{
				Delta = y1 - y0;    // Must be non-zero, or masks would have common bit.
				x0 -= y0 * (x1 - x0) / Delta;
				z0 -= y0 * (z1 - z0) / Delta;
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
				x1 -= y1 * (x0 - x1) / Delta;
				z1 -= y1 * (z0 - z1) / Delta;
				y1 = 0;

				Mask1 &= 251;
				if (x1 < 0)
					Mask1 |= 1;
				else if (x1 > this.wm1)
					Mask1 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			// Bottom edge:

			if ((Mask0 & 8) != 0)
			{
				Delta = y1 - y0;    // Must be non-zero, or masks would have common bit.
				Delta2 = this.hm1 - y0;
				x0 += Delta2 * (x1 - x0) / Delta;
				z0 += Delta2 * (z1 - z0) / Delta;
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
				x1 += Delta2 * (x0 - x1) / Delta;
				z1 += Delta2 * (z0 - z1) / Delta;
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

		public void Polygon(Vector4[] Nodes, SKColor Color)
		{
			int i, c = Nodes.Length;
			if (c < 3)
				return;

			int MinY = 0;
			int MaxY = 0;
			int Y;
			Vector4 P;

			Nodes = (Vector4[])Nodes.Clone();
			for (i = 0; i < c; i++)
			{
				Nodes[i] = P = Vector4.Transform(Nodes[i], this.t);

				if (i == 0)
					MinY = MaxY = (int)(this.cy - P.Y / P.W + 0.5f);
				else
				{
					Y = (int)(this.cy - P.Y / P.W + 0.5f);

					if (Y < MinY)
						MinY = Y;
					else if (Y > MaxY)
						MaxY = Y;
				}
			}

			if (MaxY < 0)
				return;
			else if (MinY < 0)
				MinY = 0;

			if (MinY >= this.h)
				return;
			else if (MaxY >= this.h)
				MaxY = this.h - 1;

			int NrRecs = MaxY - MinY + 1;
			ScanLineRec[] Recs = new ScanLineRec[NrRecs];
			ScanLineRec Rec;

			Vector4 Last;
			Vector4 Current = Nodes[c - 1];
			Vector3 N = CalcNormal(ToVector3(Nodes[0]), ToVector3(Nodes[1]), ToVector3(Current));
			float x0, y0, z0;
			float x1, y1, z1;
			float w;
			float dx, dy, dz;

			for (i = 0; i < c; i++)
			{
				Last = Current;
				Current = Nodes[i];

				w = 1.0f / Last.W;
				x0 = this.cx + Last.X * w;
				y0 = this.cy - Last.Y * w;
				z0 = Last.Z;

				w = 1.0f / Current.W;
				x1 = this.cx + Current.X * w;
				y1 = this.cy - Current.Y * w;
				z1 = Current.Z;

				if (!this.ClipTopBottom(ref x0, ref y0, ref z0, ref x1, ref y1, ref z1))
					continue;

				if (y0 == y1)
				{
					this.AddNode(Recs, MinY, x0, y0, z0);
					this.AddNode(Recs, MinY, x1, y1, z1);
				}
				else
				{
					if (y1 < y0)
					{
						w = x0;
						x0 = x1;
						x1 = w;

						w = y0;
						y0 = y1;
						y1 = w;

						w = z0;
						z0 = z1;
						z1 = w;
					}

					dy = y1 - y0;
					dx = (x1 - x0) / dy;
					dz = (z1 - z0) / dy;

					w = 1 - (y0 - ((int)y0));
					y0 += w;
					x0 += dx * w;
					z0 += dz * w;

					y1--;
					while (y0 <= y1)
					{
						this.AddNode(Recs, MinY, x0, y0, z0);
						y0++;
						x0 += dx;
						z0 += dz;
					}
					y1++;

					w = y1 - ((int)y1);
					if (w > 0)
					{
						y0 += w;
						x0 += dx * w;
						z0 += dz * w;

						this.AddNode(Recs, MinY, x0, y0, z0);
					}
				}
			}

			for (i = 0; i < NrRecs; i++)
			{
				Rec = Recs[i];
				if (Rec == null)
					continue;

				Y = i + MinY;

				// TODO: Color interpolation

				if (Rec.nodes != null)
				{
					bool First = true;

					x0 = z0 = 0;
					foreach (KeyValuePair<float, float> Rec2 in Rec.nodes)
					{
						if (First)
						{
							First = false;
							x0 = Rec2.Key;
							z0 = Rec2.Value;
						}
						else
						{
							this.ScanLine(x0, Y, z0, Color, Rec2.Key, Rec2.Value, Color);
							First = true;
						}
					}

					if (!First)
						this.Plot((int)(x0 + 0.5f), Y, z0, ToUInt(Color));
				}
				else if (Rec.x1.HasValue)
					this.ScanLine(Rec.x0, Y, Rec.z0, Color, Rec.x1.Value, Rec.z1.Value, Color);
				else
					this.Plot((int)(Rec.x0 + 0.5f), Y, Rec.z0, ToUInt(Color));
			}
		}

		private void AddNode(ScanLineRec[] Records, int MinY, float x, float y, float z)
		{
			int i = (int)(y + 0.5f) - MinY;
			ScanLineRec Rec = Records[i];

			if (Rec == null)
			{
				Records[i] = new ScanLineRec()
				{
					x0 = x,
					z0 = z
				};
			}
			else if (!Rec.x1.HasValue)
			{
				if (x < Rec.x0)
				{
					Rec.x1 = Rec.x0;
					Rec.z1 = Rec.z0;
					Rec.x0 = x;
					Rec.z0 = z;
				}
				else
				{
					Rec.x1 = x;
					Rec.z1 = z;
				}
			}
			else
			{
				if (Rec.nodes == null)
				{
					Rec.nodes = new LinkedList<KeyValuePair<float, float>>();
					Rec.nodes.AddLast(new KeyValuePair<float, float>(Rec.x0, Rec.z0));
					Rec.nodes.AddLast(new KeyValuePair<float, float>(Rec.x1.Value, Rec.z1.Value));
				}

				LinkedListNode<KeyValuePair<float, float>> Loop = Rec.nodes.First;
				LinkedListNode<KeyValuePair<float, float>> Prev = null;

				while (Loop != null && Loop.Value.Key < x)
				{
					Prev = Loop;
					Loop = Loop.Next;
				}

				if (Loop == null)
					Rec.nodes.AddLast(new KeyValuePair<float, float>(x, z));
				else if (Prev == null)
					Rec.nodes.AddFirst(new KeyValuePair<float, float>(x, z));
				else
					Rec.nodes.AddAfter(Prev, new KeyValuePair<float, float>(x, z));
			}
		}

		private class ScanLineRec
		{
			public float x0;
			public float z0;
			public float? x1;
			public float? z1;
			public LinkedList<KeyValuePair<float, float>> nodes;
		}

		#endregion

	}
}
