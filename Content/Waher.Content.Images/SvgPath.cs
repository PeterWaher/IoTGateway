using SkiaSharp;
using System;
using System.Text;

namespace Waher.Content.Images
{
	internal enum SvgPathState
	{
		ExpectCommand,
		ExpectNumber
	}

	/// <summary>
	/// Processes SVG paths.
	/// </summary>
	public static class SvgPath
	{
		/// <summary>
		/// Parses an SVG Path and generates an image from it.
		/// </summary>
		/// <param name="SvgPath">SVG Path</param>
		/// <param name="Width">Width of image.</param>
		/// <param name="Height">Height of image.</param>
		/// <param name="Foreground">Foreground color.</param>
		/// <param name="Background">Background color.</param>
		/// <returns>Generated image.</returns>
		public static SKImage SvgPathToImage(string SvgPath, int Width, int Height,
			SKColor Foreground, SKColor Background)
		{
			return SvgPathToImage(SvgPath, Width, Height, Foreground, Background, 0, 0, 1, 1);
		}

		/// <summary>
		/// Parses an SVG Path and generates an image from it.
		/// </summary>
		/// <param name="SvgPath">SVG Path</param>
		/// <param name="Width">Width of image.</param>
		/// <param name="Height">Height of image.</param>
		/// <param name="Foreground">Foreground color.</param>
		/// <param name="Background">Background color.</param>
		/// <param name="OffsetX">X-offset of path in image.</param>
		/// <param name="OffsetY">Y-offset of path in image.</param>
		/// <param name="ScaleX">Scaling factor for x-coordinates of path in image.</param>
		/// <param name="ScaleY">Scaling factor for y-coordinates of path in image.</param>
		/// <returns>Generated image.</returns>
		public static SKImage SvgPathToImage(string SvgPath, int Width, int Height,
			SKColor Foreground, SKColor Background, float OffsetX, float OffsetY, float ScaleX, float ScaleY)
		{
			SKSurface Surface = SKSurface.Create(new SKImageInfo(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
			try
			{
				SKCanvas Canvas = Surface.Canvas;
				Canvas.Clear(Background);

				SKPath Path = Parse(SvgPath, OffsetX, OffsetY, ScaleX, ScaleY);

				SKPaint IconFill = new SKPaint()
				{
					FilterQuality = SKFilterQuality.High,
					IsAntialias = true,
					Style = SKPaintStyle.Fill,
					Color = Foreground
				};

				Canvas.DrawPath(Path, IconFill);

				return Surface.Snapshot();
			}
			finally
			{
				Surface.Dispose();
			}
		}

		/// <summary>
		/// Parses an SVG Path and generates a bitmap image from it.
		/// </summary>
		/// <param name="SvgPath">SVG Path</param>
		/// <param name="Width">Width of bitmap image.</param>
		/// <param name="Height">Height of bitmap image.</param>
		/// <param name="Foreground">Foreground color.</param>
		/// <param name="Background">Background color.</param>
		/// <returns>Generated image.</returns>
		public static SKBitmap SvgPathToBitmap(string SvgPath, int Width, int Height,
			SKColor Foreground, SKColor Background)
		{
			return SvgPathToBitmap(SvgPath, Width, Height, Foreground, Background, 0, 0, 1, 1);
		}

		/// <summary>
		/// Parses an SVG Path and generates a bitmap image from it.
		/// </summary>
		/// <param name="SvgPath">SVG Path</param>
		/// <param name="Width">Width of bitmap image.</param>
		/// <param name="Height">Height of bitmap image.</param>
		/// <param name="Foreground">Foreground color.</param>
		/// <param name="Background">Background color.</param>
		/// <param name="OffsetX">X-offset of path in image.</param>
		/// <param name="OffsetY">Y-offset of path in image.</param>
		/// <param name="ScaleX">Scaling factor for x-coordinates of path in image.</param>
		/// <param name="ScaleY">Scaling factor for y-coordinates of path in image.</param>
		/// <returns>Generated image.</returns>
		public static SKBitmap SvgPathToBitmap(string SvgPath, int Width, int Height,
			SKColor Foreground, SKColor Background, float OffsetX, float OffsetY, float ScaleX, float ScaleY)
		{
			using (SKImage Image = SvgPathToImage(SvgPath, Width, Height, Foreground, Background, OffsetX, OffsetY, ScaleX, ScaleY))
			{
				return SKBitmap.FromImage(Image);
			}
		}

		/// <summary>
		/// Parses an SVG Path.
		/// 
		/// Reference:
		/// https://developer.mozilla.org/en-US/docs/Web/SVG/Tutorial/Paths
		/// </summary>
		/// <param name="Path">Path string.</param>
		/// <returns>Parsed path</returns>
		public static SKPath Parse(string Path)
		{
			return Parse(Path, 0, 0, 1, 1);
		}

		/// <summary>
		/// Parses an SVG Path.
		/// 
		/// Reference:
		/// https://developer.mozilla.org/en-US/docs/Web/SVG/Tutorial/Paths
		/// </summary>
		/// <param name="Path">Path string.</param>
		/// <param name="OffsetX">X-offset of path in image.</param>
		/// <param name="OffsetY">Y-offset of path in image.</param>
		/// <param name="ScaleX">Scaling factor for x-coordinates of path in image.</param>
		/// <param name="ScaleY">Scaling factor for y-coordinates of path in image.</param>
		/// <returns>Parsed path</returns>
		public static SKPath Parse(string Path, float OffsetX, float OffsetY, float ScaleX, float ScaleY)
		{
			SKPath Result = new SKPath();
			SvgPathState State = SvgPathState.ExpectCommand;
			StringBuilder sb = new StringBuilder();
			float[] Numbers = new float[7];
			float LastX = 0;
			float LastY = 0;
			int NumberIndex = 0;
			int NrNumbers = 0;
			char Command = (char)0;
			bool Again;

			foreach (char ch in Path)
			{
				do
				{
					Again = false;
					switch (State)
					{
						case SvgPathState.ExpectCommand:
							Command = ch;
							switch (Command)
							{
								case ' ':
								case '\t':
								case '\a':
								case '\b':
								case '\v':
								case '\f':
								case '\r':
								case '\n':
								case ',':
									break;

								case 'M':   // Move-to Absolute
								case 'm':   // Move-to Relative
								case 'L':   // Line-to Absolute
								case 'l':   // Line-to Relative
									NrNumbers = 2;
									NumberIndex = 0;
									State = SvgPathState.ExpectNumber;
									break;

								case 'H':   // Horizontal Line-to Absolute
								case 'h':   // Horizontal Line-to Relative
								case 'V':   // Vertical Line-to Absolute
								case 'v':   // Vertical Line-to Relative
									NrNumbers = 1;
									NumberIndex = 0;
									State = SvgPathState.ExpectNumber;
									break;

								case 'Z':   // Close loop
									AddCommand(Result, Command, Numbers, ref LastX, ref LastY, OffsetX, OffsetY, ScaleX, ScaleY);
									break;

								case 'C':   // Bezier curve Absolute
								case 'c':   // Bezier curve Relative
									NrNumbers = 6;
									NumberIndex = 0;
									State = SvgPathState.ExpectNumber;
									break;

								case 'S':   // Short-hand Bezier curve Absolute
								case 's':   // Short-hand Bezier curve Relative
									Numbers[0] = 2 * Numbers[4] - Numbers[2];
									Numbers[1] = 2 * Numbers[5] - Numbers[3];
									NrNumbers = 4;
									NumberIndex = 2;
									State = SvgPathState.ExpectNumber;
									break;

								case 'Q':   // Quadratic curve Absolute
								case 'q':   // Quadratic curve Relative
									NrNumbers = 4;
									NumberIndex = 0;
									State = SvgPathState.ExpectNumber;
									break;

								case 'T':   // Short-hand Quadratic curve Absolute
								case 't':   // Short-hand Quadratic curve Relative
									Numbers[0] = 2 * Numbers[2] - Numbers[0];
									Numbers[1] = 2 * Numbers[3] - Numbers[1];
									NrNumbers = 2;
									NumberIndex = 2;
									State = SvgPathState.ExpectNumber;
									break;

								case 'A':   // Arc Absolute
								case 'a':   // Arc Relative
									NrNumbers = 7;
									NumberIndex = 0;
									State = SvgPathState.ExpectNumber;
									break;

								default:
									Result.Dispose();
									throw new Exception("Unexpected draw command: " + Command);
							}
							break;

						case SvgPathState.ExpectNumber:
							switch (ch)
							{
								case ' ':
								case '\t':
								case '\a':
								case '\b':
								case '\v':
								case '\f':
								case '\r':
								case '\n':
								case ',':
									if (!CommonTypes.TryParse(sb.ToString(), out float f))
									{
										Result.Dispose();
										throw new Exception("Invalid number: " + sb.ToString());
									}

									sb.Clear();
									Numbers[NumberIndex++] = f;
									NrNumbers--;

									if (NrNumbers == 0)
									{
										AddCommand(Result, Command, Numbers, ref LastX, ref LastY, OffsetX, OffsetY, ScaleX, ScaleY);
										State = SvgPathState.ExpectCommand;
									}
									break;

								case '+':
								case '-':
								case '.':
								case '0':
								case '1':
								case '2':
								case '3':
								case '4':
								case '5':
								case '6':
								case '7':
								case '8':
								case '9':
								case 'e':
								case 'E':
									sb.Append(ch);
									break;

								default:
									if (!CommonTypes.TryParse(sb.ToString(), out f))
									{
										Result.Dispose();
										throw new Exception("Invalid number: " + sb.ToString());
									}

									sb.Clear();
									Numbers[NumberIndex++] = f;
									NrNumbers--;

									if (NrNumbers != 0)
										throw new Exception("Unexpected numerical character: " + ch);

									AddCommand(Result, Command, Numbers, ref LastX, ref LastY, OffsetX, OffsetY, ScaleX, ScaleY);
									State = SvgPathState.ExpectCommand;

									Again = true;
									break;
							}
							break;

						default:
							Result.Dispose();
							throw new Exception("Unexpected state: " + State.ToString());
					}
				}
				while (Again);
			}

			return Result;
		}

		private static void AddCommand(SKPath Path, char Command, float[] Numbers,
			ref float LastX, ref float LastY, float OffsetX, float OffsetY, float ScaleX, float ScaleY)
		{
			switch (Command)
			{
				case 'M':   // Move-to Absolute
					LastX = Numbers[0] * ScaleX + OffsetX;
					LastY = Numbers[1] * ScaleY + OffsetY;
					Path.MoveTo(LastX, LastY);
					break;

				case 'm':   // Move-to Relative
					LastX += Numbers[0] * ScaleX;
					LastY += Numbers[1] * ScaleY;
					Path.MoveTo(LastX, LastY);
					break;

				case 'L':   // Line-to Absolute
					LastX = Numbers[0] * ScaleX + OffsetX;
					LastY = Numbers[1] * ScaleY + OffsetY;
					Path.LineTo(LastX, LastY);
					break;

				case 'l':   // Line-to Relative
					LastX += Numbers[0] * ScaleX;
					LastY += Numbers[1] * ScaleY;
					Path.LineTo(LastX, LastY);
					break;

				case 'H':   // Horizontal Line-to Absolute
					LastX = Numbers[0] * ScaleX + OffsetX;
					Path.LineTo(LastX, LastY);
					break;

				case 'h':   // Horizontal Line-to Relative
					LastX += Numbers[0] * ScaleX;
					Path.LineTo(LastX, LastY);
					break;

				case 'V':   // Vertical Line-to Absolute
					LastY = Numbers[0] * ScaleY + OffsetY;
					Path.LineTo(LastX, LastY);
					break;

				case 'v':   // Vertical Line-to Relative
					LastY += Numbers[0] * ScaleY;
					Path.LineTo(LastX, LastY);
					break;

				case 'Z':   // Close loop
					Path.Close();
					break;

				case 'C':   // Bezier curve Absolute
				case 'S':   // Short-hand Bezier curve Absolute
					float x1 = Numbers[0] * ScaleX + OffsetX;
					float y1 = Numbers[1] * ScaleY + OffsetY;
					float x2 = Numbers[2] * ScaleX + OffsetX;
					float y2 = Numbers[3] * ScaleY + OffsetY;
					LastX = Numbers[4] * ScaleX + OffsetX;
					LastY = Numbers[5] * ScaleY + OffsetY;
					Path.CubicTo(x1, y1, x2, y2, LastX, LastY);
					break;

				case 'c':   // Bezier curve Relative
				case 's':   // Short-hand Bezier curve Relative
					x1 = LastX + Numbers[0] * ScaleX;
					y1 = LastY + Numbers[1] * ScaleY;
					x2 = LastX + Numbers[2] * ScaleX;
					y2 = LastY + Numbers[3] * ScaleY;
					
					LastX += Numbers[4] * ScaleX;
					LastY += Numbers[5] * ScaleY;

					Path.CubicTo(x1, y1, x2, y2, LastX, LastY);
					break;

				case 'Q':   // Quadratic curve Absolute
				case 'T':   // Short-hand Quadratic curve Absolute
					x1 = Numbers[0] * ScaleX + OffsetX;
					y1 = Numbers[1] * ScaleY + OffsetY;
					LastX = Numbers[2] * ScaleX + OffsetX;
					LastY = Numbers[3] * ScaleY + OffsetY;
					Path.QuadTo(x1, y1, LastX, LastY);
					break;

				case 'q':   // Quadratic curve Relative
				case 't':   // Short-hand Quadratic curve Relative
					x1 = LastX + Numbers[0] * ScaleX;
					y1 = LastY + Numbers[1] * ScaleY;

					LastX += Numbers[2] * ScaleX;
					LastY += Numbers[3] * ScaleY;

					Path.QuadTo(x1, y1, LastX, LastY);
					break;

				case 'A':   // Arc Absolute
					LastX = Numbers[5] * ScaleX + OffsetX;
					LastY = Numbers[6] * ScaleY + OffsetY;
					Path.ArcTo(Numbers[0] * ScaleX, Numbers[1] * ScaleY, Numbers[2],
						Numbers[3] == 0 ? SKPathArcSize.Large : SKPathArcSize.Small,
						Numbers[4] == 0 ? SKPathDirection.CounterClockwise : SKPathDirection.Clockwise,
						LastX, LastY);
					break;

				case 'a':   // Arc Relative
					LastX += Numbers[5] * ScaleX;
					LastY += Numbers[6] * ScaleY;
					Path.ArcTo(Numbers[0] * ScaleX, Numbers[1] * ScaleY, Numbers[2],
						Numbers[3] == 0 ? SKPathArcSize.Small : SKPathArcSize.Large,
						Numbers[4] == 0 ? SKPathDirection.CounterClockwise : SKPathDirection.Clockwise,
						LastX, LastY);
					break;
			}
		}

	}
}
