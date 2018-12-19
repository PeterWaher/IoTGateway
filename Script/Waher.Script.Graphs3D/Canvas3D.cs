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
		private float[] xBuf;
		private float[] yBuf;
		private float[] zBuf;
		private Vector3[] normalBuf;
		private SKColor[] colorBuf;
		private float distance = 0;
		private Vector3 viewerPosition = Vector3.Zero;
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
				throw new ArgumentOutOfRangeException("Width must be a positive integer.", nameof(Width));

			if (Height <= 0)
				throw new ArgumentOutOfRangeException("Height must be a positive integer.", nameof(Height));

			if (Oversampling <= 0)
				throw new ArgumentOutOfRangeException("Oversampling must be a positive integer.", nameof(Oversampling));

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
			this.xBuf = new float[this.w];
			this.yBuf = new float[this.w];
			this.zBuf = new float[this.w];
			this.normalBuf = new Vector3[this.w];
			this.colorBuf = new SKColor[this.w];

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
			this.distance = 0;
			this.viewerPosition = new Vector3(this.cx, this.cy, 0);
			this.t = new Matrix4x4(this.overSampling, 0, 0, 0, 0, this.overSampling, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
		}

		/// <summary>
		/// Multiplies a projection matrix (along the Z-axis) to the current transformation.
		/// </summary>
		/// <param name="Distance">Distance between projection plane and camera.</param>
		public void ProjectZ(float Distance)
		{
			if (Distance <= 0)
				throw new ArgumentOutOfRangeException("Invalid camera distance.", nameof(Distance));

			this.distance = Distance;
			this.viewerPosition = new Vector3(this.cx, this.cy, -this.distance);
		}

		/// <summary>
		/// Distance from projection plane.
		/// </summary>
		public float Distance => this.distance;

		/// <summary>
		/// Viewer position.
		/// </summary>
		public Vector3 ViewerPosition => this.viewerPosition;

		/// <summary>
		/// Transforms a world coordinate to a display coordinate.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <returns>Transformed point.</returns>
		public Vector4 Transform(Vector4 Point)
		{
			float x, y;

			Point = Vector4.Transform(Point, this.t);
			if (this.distance > 0)
			{
				float d = this.distance / (Point.Z + this.distance);
				x = this.cx + (int)(Point.X * d + 0.5f);
				y = this.cy - (int)(Point.Y * d + 0.5f);
			}
			else
			{
				x = this.cx + (int)(Point.X + 0.5f);
				y = this.cy - (int)(Point.Y + 0.5f);
			}

			return new Vector4(x, y, Point.Z, 1);
		}

		/// <summary>
		/// Transforms a world coordinate to a display coordinate.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <returns>Transformed point.</returns>
		public Vector3 Transform(Vector3 Point)
		{
			float x, y;

			Point = Vector3.Transform(Point, this.t);
			if (this.distance > 0)
			{
				float d = this.distance / (Point.Z + this.distance);
				x = this.cx + (int)(Point.X * d + 0.5f);
				y = this.cy - (int)(Point.Y * d + 0.5f);
			}
			else
			{
				x = this.cx + (int)(Point.X + 0.5f);
				y = this.cy - (int)(Point.Y + 0.5f);
			}

			return new Vector3(x, y, Point.Z);
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
			int x, y;

			this.last = Point;

			Point = Vector4.Transform(Point, this.t);
			if (Point.Z >= 0)
			{
				if (this.distance > 0)
				{
					float d = this.distance / (Point.Z + this.distance);
					x = this.cx + (int)(Point.X * d + 0.5f);
					y = this.cy - (int)(Point.Y * d + 0.5f);
				}
				else
				{
					x = this.cx + (int)(Point.X + 0.5f);
					y = this.cy - (int)(Point.Y + 0.5f);
				}

				if (x >= 0 && x < this.w && y >= 0 && y < this.h)
					this.Plot(x, y, Point.Z, Color);
			}
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

			// TODO: Clip z=0

			float x0, y0, z0;
			float x1, y1, z1;
			float temp;

			if (this.distance > 0)
			{
				z0 = P0.Z;
				temp = this.distance / (z0 + this.distance);
				x0 = this.cx + P0.X * temp;
				y0 = this.cy - P0.Y * temp;

				z1 = P1.Z;
				temp = 1.0f / (z1 + this.distance);
				x1 = this.cx + P1.X * this.distance * temp;
				y1 = this.cy - P1.Y * this.distance * temp;
			}
			else
			{
				x0 = this.cx + P0.X;
				y0 = this.cy - P0.Y;
				z0 = P0.Z;
				x1 = this.cx + P1.X;
				y1 = this.cy - P1.Y;
				z1 = P1.Z;
			}


			if (this.ClipLine(ref x0, ref y0, ref z0, ref x1, ref y1, ref z1))
			{
				float dx = x1 - x0;
				float dy = y1 - y0;
				float dz;

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

		private void ScanLine(float x0, float y0, float z0, float x1, float z1, Vector3 Normal, I3DShader Shader)
		{
			float Delta;

			if (x1 < x0)
			{
				Delta = x0;
				x0 = x1;
				x1 = Delta;

				Delta = z0;
				z0 = z1;
				z1 = Delta;
			}

			float y1 = y0;
			float x0b = x0;
			float x1b = x1;

			if (!this.ClipLine(ref x0, ref y0, ref z0, ref x1, ref y1, ref z1))
				return;

			if (x0 == x1)
			{
				if (z0 < z1)
				{
					this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0,
						ToUInt(Shader.GetColor(x0, y0, z0, Normal)));
				}
				else
				{
					this.Plot((int)(x1 + 0.5f), (int)(y0 + 0.5f), z1,
						ToUInt(Shader.GetColor(x1, y0, z1, Normal)));
				}
			}
			else
			{
				int ix0 = (int)(x0 + 0.5f);
				int ix1 = (int)(x1 + 0.5f);
				float dx = (x1 - x0);
				float dz = (z1 - z0) / dx;
				int i = 0;
				int p = (int)(y0 + 0.5f) * this.w + ix0;
				int p4 = p << 2;
				int c;
				SKColor cl;
				byte A;
				byte R2, G2, B2, A2;
				byte R3, G3, B3, A3;

				while (ix0 <= ix1)
				{
					this.xBuf[i] = ix0++;
					this.yBuf[i] = y0;      // TODO: Correct y and z values. Take projection into account.
					this.zBuf[i] = z0;
					this.normalBuf[i++] = Normal;
					z0 += dz;
				}

				c = i;
				Shader.GetColors(this.xBuf, this.yBuf, this.zBuf, this.normalBuf, c, this.colorBuf);

				for (i = 0; i < c; i++)
				{
					z0 = this.zBuf[i];

					if (z0 > 0 && z0 < this.zBuffer[p])
					{
						this.zBuffer[p++] = z0;

						cl = this.colorBuf[i];

						if ((A = cl.Alpha) == 255)
						{
							this.pixels[p4++] = cl.Red;
							this.pixels[p4++] = cl.Green;
							this.pixels[p4++] = cl.Blue;
							this.pixels[p4++] = 255;
						}
						else
						{
							R2 = this.pixels[p4++];
							G2 = this.pixels[p4++];
							B2 = this.pixels[p4++];
							A2 = this.pixels[p4];

							if (A2 == 255)
							{
								R3 = (byte)(((cl.Red * A + R2 * (255 - A)) + 128) / 255);
								G3 = (byte)(((cl.Green * A + G2 * (255 - A)) + 128) / 255);
								B3 = (byte)(((cl.Blue * A + B2 * (255 - A)) + 128) / 255);
								A3 = 255;
							}
							else
							{
								R2 = (byte)((R2 * A2 + 128) / 255);
								G2 = (byte)((G2 * A2 + 128) / 255);
								B2 = (byte)((B2 * A2 + 128) / 255);

								R3 = (byte)(((cl.Red * A + R2 * (255 - A)) + 128) / 255);
								G3 = (byte)(((cl.Green * A + G2 * (255 - A)) + 128) / 255);
								B3 = (byte)(((cl.Blue * A + B2 * (255 - A)) + 128) / 255);
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
				}
			}
		}

		#endregion

		#region Polygon

		private static Vector3 ToVector3(Vector4 P)
		{
			return new Vector3(P.X, P.Y, P.Z);
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

		/// <summary>
		/// Draws a closed polygon.
		/// </summary>
		/// <param name="Nodes">Nodes.</param>
		/// <param name="Color">Color</param>
		public void Polygon(Vector4[] Nodes, SKColor Color)
		{
			this.Polygons(new Vector4[][] { Nodes }, new ConstantColor(Color));
		}

		/// <summary>
		/// Draws a closed polygon.
		/// </summary>
		/// <param name="Nodes">Nodes.</param>
		/// <param name="Shader">Shader.</param>
		public void Polygon(Vector4[] Nodes, I3DShader Shader)
		{
			this.Polygons(new Vector4[][] { Nodes }, Shader);
		}

		/// <summary>
		/// Draws a set of closed polygons. Interior polygons can be used to undraw the corresponding sections.
		/// </summary>
		/// <param name="Nodes">Nodes.</param>
		/// <param name="Color">Color</param>
		public void Polygons(Vector4[][] Nodes, SKColor Color)
		{
			this.Polygons(Nodes, new ConstantColor(Color));
		}

		/// <summary>
		/// Draws a set of closed polygons. Interior polygons can be used to undraw the corresponding sections.
		/// </summary>
		/// <param name="Nodes">Nodes.</param>
		/// <param name="Shader">Shader.</param>
		public void Polygons(Vector4[][] Nodes, I3DShader Shader)
		{
			int j, d = Nodes.Length;
			int i, c;
			int MinY = 0;
			int MaxY = 0;
			int Y;
			Vector4 P;
			Vector4[] v;
			bool First = true;

			Shader.Transform(this);
			Nodes = (Vector4[][])Nodes.Clone();

			d = Nodes.Length;
			for (j = 0; j < d; j++)
			{
				v = (Vector4[])Nodes[j].Clone();
				Nodes[j] = v;

				c = v.Length;
				if (c < 3)
					continue;

				for (i = 0; i < c; i++)
				{
					v[i] = P = Vector4.Transform(v[i], this.t);

					if (this.distance > 0)
						Y = (int)(this.cy - P.Y * this.distance / (P.Z + this.distance) + 0.5f);
					else
						Y = (int)(this.cy - P.Y + 0.5f);

					if (First)
					{
						First = false;
						MinY = MaxY = Y;
					}
					else if (Y < MinY)
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
				MaxY = this.hm1;

			int NrRecs = MaxY - MinY + 1;
			ScanLineRec[] Recs = new ScanLineRec[NrRecs];
			ScanLineRec Rec;
			Vector4 Last;
			Vector4 Current;
			Vector3 N;
			float x0, y0, z0;
			float x1, y1, z1;
			float w;
			float dx, dy, dz;
			int iy0, iy1;

			for (j = 0; j < d; j++)
			{
				v = Nodes[j];
				c = v.Length;

				if (c < 3)
					continue;

				Last = v[c - 2];
				Current = v[c - 1];

				N = CalcNormal(ToVector3(v[0]), ToVector3(v[1]), ToVector3(Current));

				if (this.distance > 0)
				{
					y0 = this.cy - Last.Y * this.distance / (Last.Z + this.distance);
					y1 = this.cy - Current.Y * this.distance / (Current.Z + this.distance);
				}
				else
				{
					y0 = this.cy - Last.Y;
					y1 = this.cy - Current.Y;
				}

				iy0 = (int)(y0 + 0.5f);
				iy1 = (int)(y1 + 0.5f);

				int LastDir;
				int Dir = Math.Sign(iy1 - iy0);

				for (i = 0; i < c; i++)
				{
					Last = Current;
					Current = v[i];

					if (this.distance > 0)
					{
						w = this.distance / (Last.Z + this.distance);
						x0 = this.cx + Last.X * w;
						y0 = this.cy - Last.Y * w;
						z0 = Last.Z;

						w = this.distance / (Current.Z + this.distance);
						x1 = this.cx + Current.X * w;
						y1 = this.cy - Current.Y * w;
						z1 = Current.Z;
					}
					else
					{
						x0 = this.cx + Last.X;
						y0 = this.cy - Last.Y;
						z0 = Last.Z;

						x1 = this.cx + Current.X;
						y1 = this.cy - Current.Y;
						z1 = Current.Z;
					}

					if (!this.ClipTopBottom(ref x0, ref y0, ref z0, ref x1, ref y1, ref z1))
						continue;

					iy0 = (int)(y0 + 0.5f);
					iy1 = (int)(y1 + 0.5f);

					LastDir = Dir;
					Dir = Math.Sign(iy1 - iy0);

					if (Dir == 0)
						continue;

					dy = y1 - y0;
					dx = (x1 - x0) / dy;
					dz = (z1 - z0) / dy;

					if (Dir > 0)
					{
						if (Dir == LastDir)
						{
							iy0++;
							x0 += dx;
							z0 += dz;
						}

						while (iy0 <= iy1)
						{
							this.AddNode(Recs, MinY, x0, iy0, z0, N);

							iy0++;
							x0 += dx;
							z0 += dz;
						}
					}
					else
					{
						if (Dir == LastDir)
						{
							iy0--;
							x0 -= dx;
							z0 -= dz;
						}

						while (iy0 >= iy1)
						{
							this.AddNode(Recs, MinY, x0, iy0, z0, N);

							iy0--;
							x0 -= dx;
							z0 -= dz;
						}
					}
				}
			}

			for (i = 0; i < NrRecs; i++)
			{
				Rec = Recs[i];
				if (Rec is null)
					continue;

				Y = i + MinY;

				if (Rec.nodes != null)
				{
					First = true;

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
							this.ScanLine(x0, Y, z0, Rec2.Key, Rec2.Value, Rec.n, Shader);
							First = true;
						}
					}

					if (!First)
						this.Plot((int)(x0 + 0.5f), Y, z0, ToUInt(Shader.GetColor(x0, Y, z0, Rec.n)));
				}
				else if (Rec.x1.HasValue)
					this.ScanLine(Rec.x0, Y, Rec.z0, Rec.x1.Value, Rec.z1.Value, Rec.n, Shader);
				else
					this.Plot((int)(Rec.x0 + 0.5f), Y, Rec.z0, ToUInt(Shader.GetColor(Rec.x0, Y, Rec.z0, Rec.n)));
			}
		}

		private void AddNode(ScanLineRec[] Records, int MinY, float x, float y, float z, Vector3 N)
		{
			int i = (int)(y + 0.5f) - MinY;
			ScanLineRec Rec = Records[i];

			if (Rec is null)
			{
				Records[i] = new ScanLineRec()
				{
					x0 = x,
					z0 = z,
					n = N
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
				if (Rec.nodes is null)
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

				if (Loop is null)
					Rec.nodes.AddLast(new KeyValuePair<float, float>(x, z));
				else if (Prev is null)
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
			public Vector3 n;
		}

		#endregion

		#region Text

		/// <summary>
		/// Draws text on the canvas.
		/// </summary>
		/// <param name="Text">Text to draw.</param>
		/// <param name="Start">Start position.</param>
		/// <param name="FontFamily">Font family.</param>
		/// <param name="TextSize">Text size.</param>
		/// <param name="Color">Text color.</param>
		public void Text(string Text, Vector4 Start, string FontFamily, float TextSize, SKColor Color)
		{
			this.Text(Text, Start, FontFamily, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
				SKFontStyleSlant.Upright, TextSize, Color);
		}

		/// <summary>
		/// Draws text on the canvas.
		/// </summary>
		/// <param name="Text">Text to draw.</param>
		/// <param name="Start">Start position.</param>
		/// <param name="FontFamily">Font family.</param>
		/// <param name="Weight">Font weight.</param>
		/// <param name="TextSize">Text size.</param>
		/// <param name="Color">Text color.</param>
		public void Text(string Text, Vector4 Start, string FontFamily, SKFontStyleWeight Weight,
			float TextSize, SKColor Color)
		{
			this.Text(Text, Start, FontFamily, Weight, SKFontStyleWidth.Normal,
				SKFontStyleSlant.Upright, TextSize, Color);
		}

		/// <summary>
		/// Draws text on the canvas.
		/// </summary>
		/// <param name="Text">Text to draw.</param>
		/// <param name="Start">Start position.</param>
		/// <param name="FontFamily">Font family.</param>
		/// <param name="Weight">Font weight.</param>
		/// <param name="Width">Font width.</param>
		/// <param name="TextSize">Text size.</param>
		/// <param name="Color">Text color.</param>
		public void Text(string Text, Vector4 Start, string FontFamily, SKFontStyleWeight Weight,
			SKFontStyleWidth Width, float TextSize, SKColor Color)
		{
			this.Text(Text, Start, FontFamily, Weight, Width, SKFontStyleSlant.Upright,
				TextSize, Color);
		}

		/// <summary>
		/// Draws text on the canvas.
		/// </summary>
		/// <param name="Text">Text to draw.</param>
		/// <param name="Start">Start position.</param>
		/// <param name="FontFamily">Font family.</param>
		/// <param name="Weight">Font weight.</param>
		/// <param name="Width">Font width.</param>
		/// <param name="Slant">Font slant.</param>
		/// <param name="TextSize">Text size.</param>
		/// <param name="Color">Text color.</param>
		public void Text(string Text, Vector4 Start, string FontFamily, SKFontStyleWeight Weight,
			SKFontStyleWidth Width, SKFontStyleSlant Slant, float TextSize, SKColor Color)
		{
			SKPaint Paint = null;
			SKPath Path = null;
			SKPath Simple = null;
			SKPath.Iterator e = null;
			SKPoint[] Points = new SKPoint[4];
			SKPathVerb Verb;

			try
			{
				Paint = new SKPaint()
				{
					Typeface = SKTypeface.FromFamilyName(FontFamily, Weight, Width, Slant),
					TextSize = TextSize
				};

				Path = Paint.GetTextPath(Text, 0, 0);
				//Simple = Path.Simplify();

				e = Path.CreateIterator(false);

				List<Vector4> P = new List<Vector4>();
				List<Vector4[]> v = new List<Vector4[]>();
				float MaxX = 0;
				float X, Y;
				float x0, x1, x2, x3;
				float y0, y1, y2, y3;
				float dx, dy, t, w, d, t2, w2, t3, w3, weight;
				int i, c;

				while ((Verb = e.Next(Points)) != SKPathVerb.Done)
				{
					switch (Verb)
					{
						case SKPathVerb.Close:
							if ((c = P.Count) > 1 && P[0] == P[c - 1])
								P.RemoveAt(c - 1);

							v.Add(P.ToArray());
							P.Clear();
							break;

						case SKPathVerb.Move:
							X = Points[0].X;
							if (X > MaxX)
							{
								if (v.Count > 0)
								{
									this.Polygons(v.ToArray(), Color);
									v.Clear();
								}

								MaxX = X;
							}

							if (P.Count > 0)
							{
								if ((c = P.Count) > 1 && P[0] == P[c - 1])
									P.RemoveAt(c - 1);

								v.Add(P.ToArray());
								P.Clear();
							}

							P.Add(new Vector4(Start.X + X, Start.Y - Points[0].Y, Start.Z, 1));
							break;

						case SKPathVerb.Line:
							X = Points[1].X;
							if (X > MaxX)
								MaxX = X;

							P.Add(new Vector4(Start.X + X, Start.Y - Points[1].Y, Start.Z, 1));
							break;

						case SKPathVerb.Quad:
							x0 = Points[0].X;
							y0 = Points[0].Y;
							if (x0 > MaxX)
								MaxX = x0;

							x1 = Points[1].X;
							y1 = Points[1].Y;
							if (x1 > MaxX)
								MaxX = x1;

							x2 = Points[2].X;
							y2 = Points[2].Y;
							if (x2 > MaxX)
								MaxX = x2;

							dx = x2 - x0;
							dy = y2 - y0;

							c = (int)Math.Ceiling(Math.Sqrt(dx * dx + dy * dy) / 5);
							for (i = 1; i <= c; i++)
							{
								t = ((float)i) / c;
								w = 1 - t;

								t2 = t * t;
								w2 = w * w;

								X = w2 * x0 + 2 * t * w * x1 + t2 * x2;
								Y = w2 * y0 + 2 * t * w * y1 + t2 * y2;

								P.Add(new Vector4(Start.X + X, Start.Y - Y, Start.Z, 1));
							}
							break;

						case SKPathVerb.Conic:
							x0 = Points[0].X;
							y0 = Points[0].Y;
							if (x0 > MaxX)
								MaxX = x0;

							x1 = Points[1].X;
							y1 = Points[1].Y;
							if (x1 > MaxX)
								MaxX = x1;

							x2 = Points[2].X;
							y2 = Points[2].Y;
							if (x2 > MaxX)
								MaxX = x2;

							dx = x2 - x0;
							dy = y2 - y0;

							weight = e.ConicWeight();

							c = (int)Math.Ceiling(Math.Sqrt(dx * dx + dy * dy) / 5);
							for (i = 1; i <= c; i++)
							{
								t = ((float)i) / c;
								w = 1 - t;

								t2 = t * t;
								w2 = w * w;

								d = 1.0f / (w2 + 2 * weight * t * w + t2);
								X = (w2 * x0 + 2 * weight * t * w * x1 + t2 * x2) * d;
								Y = (w2 * y0 + 2 * weight * t * w * y1 + t2 * y2) * d;

								P.Add(new Vector4(Start.X + X, Start.Y - Y, Start.Z, 1));
							}
							break;

						case SKPathVerb.Cubic:
							x0 = Points[0].X;
							y0 = Points[0].Y;
							if (x0 > MaxX)
								MaxX = x0;

							x1 = Points[1].X;
							y1 = Points[1].Y;
							if (x1 > MaxX)
								MaxX = x1;

							x2 = Points[2].X;
							y2 = Points[2].Y;
							if (x2 > MaxX)
								MaxX = x2;

							x3 = Points[3].X;
							y3 = Points[3].Y;
							if (x3 > MaxX)
								MaxX = x3;

							dx = x3 - x0;
							dy = y3 - y0;

							c = (int)Math.Ceiling(Math.Sqrt(dx * dx + dy * dy) / 5);
							for (i = 1; i <= c; i++)
							{
								t = ((float)i) / c;
								w = 1 - t;

								t2 = t * t;
								w2 = w * w;

								t3 = t2 * t;
								w3 = w2 * w;

								X = w3 * x0 + 3 * t * w2 * x1 + 3 * t2 * w * x2 + t3 * x3;
								Y = w3 * y0 + 3 * t * w2 * y1 + 3 * t2 * w * y2 + t3 * y3;

								P.Add(new Vector4(Start.X + X, Start.Y - Y, Start.Z, 1));
							}
							break;
					}
				}

				if (v.Count > 0)
					this.Polygons(v.ToArray(), Color);
			}
			finally
			{
				if (Paint != null)
					Paint.Dispose();

				if (Path != null)
					Path.Dispose();

				if (Simple != null)
					Simple.Dispose();

				if (e != null)
					e.Dispose();
			}
		}

		#endregion

	}
}
