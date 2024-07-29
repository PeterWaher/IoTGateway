using System;
using System.Numerics;
using System.Text;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Fractals.ComplexFractals
{
	/// <summary>
	/// Calculates a Julia set Fractal Image
	/// </summary>
	/// <example>
	/// JuliaInternalFractal(z,c,dr[,Colors][,DimX[,DimY]])
	/// JuliaInternalFractal(z,f(z),dr[,Colors][,DimX[,DimY]])
	/// 
	/// JuliaInternalFractal((0,0),(-0.65,-0.4125),3,RandomLinearAnalogousHSL(16,4),800,600)
	/// JuliaInternalFractal((0,0),(-0.785028076171875,-0.1465322265625),3,RandomLinearAnalogousHSL(16,4),800,600)
	/// 
	/// General version:
	/// JuliaInternalFractal((0,0),z->z^2+i,3,RandomLinearAnalogousHsl(16,4))
	/// JuliaInternalFractal((0,0),z->z^5+0.364716021116823,3,RandomLinearAnalogousHsl(16,4))
	/// JuliaInternalFractal((0,0),z->sin(z),3,RandomLinearAnalogousHsl(16,4))
	/// JuliaInternalFractal((0,0),z->(1,0.1)*sin(z),7,RandomLinearAnalogousHsl(16,4))
	/// JuliaInternalFractal((0,0),z->(1,0.2)*sin(z),7,RandomLinearAnalogousHsl(16,4))
	/// JuliaInternalFractal((0,0),z->(1,0.3)*sin(z),7,RandomLinearAnalogousHsl(16,4))
	/// JuliaInternalFractal((0,0),z->(1,0.4)*sin(z),7,RandomLinearAnalogousHsl(16,4))
	/// JuliaInternalFractal((0,0),z->(1,0.5)*sin(z),7,RandomLinearAnalogousHsl(16,4))
	/// 
	/// Inzoomningar:
	/// JuliaInternalFractal((0.0437500000000002,0.00481249999999323),z->(1,0.2)*sin(z),1.75,RandomLinearAnalogousHsl(1024,16,21),320,200)
	/// </example>
	public class JuliaInternalFractal : FunctionMultiVariate
	{
		/// <summary>
		/// TODO
		/// </summary>
		public JuliaInternalFractal(ScriptNode z, ScriptNode c, ScriptNode dr, ScriptNode Palette,
			ScriptNode DimX, ScriptNode DimY, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, c, dr, Palette, DimX, DimY },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar}, Start, Length, Expression)
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public JuliaInternalFractal(ScriptNode z, ScriptNode c, ScriptNode dr, ScriptNode Palette,
			ScriptNode DimX, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, c, dr, Palette, DimX },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Vector, ArgumentType.Scalar}, Start, Length, Expression)
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public JuliaInternalFractal(ScriptNode z, ScriptNode c, ScriptNode dr, ScriptNode Palette, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, c, dr, Palette },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Vector}, Start, Length, Expression)
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public JuliaInternalFractal(ScriptNode z, ScriptNode c, ScriptNode dr, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, c, dr },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "z", "c", "dr", "Palette", "DimX", "DimY" };
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			string ColorExpression = null;
			SKColor[] Palette;
			double rc, ic;
			double r0, i0;
			double dr;
			int dimx, dimy;
			int i, c;
			object Obj;
			ILambdaExpression f;
			ScriptNode fDef = null;
			c = Arguments.Length;
			i = 0;

			Obj = Arguments[i++].AssociatedObjectValue;
			if (Obj is Complex z)
			{
				rc = z.Real;
				ic = z.Imaginary;
			}
			else
			{
				rc = Expression.ToDouble(Obj);
				ic = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			}

			if (i >= c)
				throw new ScriptRuntimeException("Insufficient parameters in call to JuliaInternalFractal().", this);

			Obj = Arguments[i++].AssociatedObjectValue;
			if (!((f = Obj as ILambdaExpression) is null))
			{
				if (f.NrArguments != 1)
					throw new ScriptRuntimeException("Lambda expression in calls to JuliaInternalFractal() must be of one variable.", this);

				r0 = 0;
				i0 = 0;
				fDef = this.Arguments[i - 1];
			}
			else if (Obj is Complex z2)
			{
				r0 = z2.Real;
				i0 = z2.Imaginary;
			}
			else
			{
				if (i >= c)
					throw new ScriptRuntimeException("Insufficient parameters in call to JuliaInternalFractal().", this);

				r0 = Expression.ToDouble(Obj);
				i0 = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			}

			if (i >= c)
				throw new ScriptRuntimeException("Insufficient parameters in call to JuliaInternalFractal().", this);

			dr = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);

			if (i < c && !(this.Arguments[i] is null) && Arguments[i] is ObjectVector)
			{
				ColorExpression = this.Arguments[i].SubExpression;
				Palette = FractalGraph.ToPalette((ObjectVector)Arguments[i++]);
			}
			else
			{
				Palette = ColorModels.RandomLinearAnalogousHSL.CreatePalette(16, 4, out int Seed, this, Variables);
				ColorExpression = "RandomLinearAnalogousHSL(16,4," + Seed.ToString() + ")";

				if (i < c && this.Arguments[i] is null)
					i++;
			}

			if (i < c)
				dimx = (int)Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			else
				dimx = 320;

			if (i < c)
				dimy = (int)Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			else
				dimy = 200;

			if (i < c)
			{
				throw new ScriptRuntimeException("Parameter mismatch in call to JuliaInternalFractal(z,c|f,dr[,Palette][,dimx[,dimy]]).",
					this);
			}

			if (dimx <= 0 || dimx > 5000 || dimy <= 0 || dimy > 5000)
				throw new ScriptRuntimeException("Image size must be within 1x1 to 5000x5000", this);

			if (!(f is null))
			{
				return CalcJulia(rc, ic, f, fDef, dr, Palette, dimx, dimy, this, Variables,
					this.FractalZoomScript, new object[] { Palette, dimx, dimy, r0, i0, ColorExpression, fDef });
			}
			else
			{
				return CalcJulia(rc, ic, r0, i0, dr, Palette, dimx, dimy, this,
					this.FractalZoomScript, new object[] { Palette, dimx, dimy, r0, i0, ColorExpression, fDef });
			}
		}

		private string FractalZoomScript(double r, double i, double Size, object State)
		{
			object[] Parameters = (object[])State;
			int DimX = (int)Parameters[1];
			int DimY = (int)Parameters[2];
			double r0 = (double)Parameters[3];
			double i0 = (double)Parameters[4];
			string ColorExpression = (string)Parameters[5];
			ScriptNode f = (ScriptNode)Parameters[6];

			StringBuilder sb = new StringBuilder();

			sb.Append("JuliaInternalFractal((");
			sb.Append(Expression.ToString(r));
			sb.Append(',');
			sb.Append(Expression.ToString(i));
			sb.Append("),");

			if (!(f is null))
				sb.Append(f.SubExpression);
			else
			{
				sb.Append('(');
				sb.Append(Expression.ToString(r0));
				sb.Append(',');
				sb.Append(Expression.ToString(i0));
				sb.Append(')');
			}

			sb.Append(',');
			sb.Append(Expression.ToString(Size / 4));

			if (!string.IsNullOrEmpty(ColorExpression))
			{
				sb.Append(',');
				sb.Append(ColorExpression);
			}

			sb.Append(',');
			sb.Append(DimX.ToString());
			sb.Append(',');
			sb.Append(DimY.ToString());
			sb.Append(')');

			return sb.ToString();
		}

		/// <summary>
		/// TODO
		/// </summary>
		public static FractalGraph CalcJulia(double rCenter, double iCenter, double R0, double I0, double rDelta,
			SKColor[] Palette, int Width, int Height, ScriptNode Node, FractalZoomScript FractalZoomScript, object State)
		{
			byte[] reds;
			byte[] greens;
			byte[] blues;
			double r0, i0, r1, i1;
			double dr, di;
			double r, i;
			double zr, zi, zrt, zr2, zi2;
			double aspect;
			int x, y;
			int n, N;
			int index;
			SKColor cl;

			N = Palette.Length;
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

			int size = Width * Height * 4;
			byte[] rgb = new byte[size];

			rDelta *= 0.5;
			r0 = rCenter - rDelta;
			r1 = rCenter + rDelta;

			aspect = ((double)Width) / Height;

			i0 = iCenter - rDelta / aspect;
			i1 = iCenter + rDelta / aspect;

			dr = (r1 - r0) / Width;
			di = (i1 - i0) / Height;

			for (y = 0, i = i0, index = 0; y < Height; y++, i += di)
			{
				for (x = 0, r = r0; x < Width; x++, r += dr)
				{
					zr = r;
					zi = i;

					n = 0;
					zr2 = zr * zr;
					zi2 = zi * zi;

					while (zr2 + zi2 < 9 && n < N)
					{
						n++;
						zrt = zr2 - zi2 + R0;
						zi = 2 * zr * zi + I0;
						zr = zrt;

						zr2 = zr * zr;
						zi2 = zi * zi;
					}

					n = (int)(N * Math.Sqrt((zr2 + zi2) / 9) + 0.5);
					if (n < N)
					{
						rgb[index++] = blues[n];
						rgb[index++] = greens[n];
						rgb[index++] = reds[n];
					}
					else
					{
						rgb[index++] = 0;
						rgb[index++] = 0;
						rgb[index++] = 0;
					}

					rgb[index++] = 255;
				}
			}

			PixelInformation Pixels = new PixelInformationRaw(SKColorType.Bgra8888, rgb, Width, Height, Width << 2);
			return new FractalGraph(Pixels, r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public static FractalGraph CalcJulia(double rCenter, double iCenter, ILambdaExpression f, ScriptNode _,
			double rDelta, SKColor[] Palette, int Width, int Height, ScriptNode Node, Variables Variables,
			FractalZoomScript FractalZoomScript, object State)
		{
			byte[] reds;
			byte[] greens;
			byte[] blues;
			double r0, i0, r1, i1;
			double dr, di;
			double r, i, Mod;
			double aspect;
			int x, x2, y;
			int n, N;
			SKColor cl;

			N = Palette.Length;
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

			int size = Width * Height * 4;
			byte[] rgb = new byte[size];
			Complex z;
			IElement[] P = new IElement[1];
			int j, c;
			IElement Obj;

			rDelta *= 0.5;
			r0 = rCenter - rDelta;
			r1 = rCenter + rDelta;

			int[] Counts = new int[N];

			aspect = ((double)Width) / Height;

			i0 = iCenter - rDelta / aspect;
			i1 = iCenter + rDelta / aspect;

			dr = (r1 - r0) / Width;
			di = (i1 - i0) / Height;

			for (y = 0, i = i0; y < Height; y++, i += di)
			{
				Complex[] Row = new Complex[Width];
				int[] Offset = new int[Width];

				c = Width;
				for (x = 0, x2 = y * Width * 4, r = r0; x < Width; x++, r += dr, x2 += 4)
				{
					Row[x] = new Complex(r, i);
					Offset[x] = x2;
				}

				Variables v = new Variables();
				Variables.CopyTo(v);

				n = 0;
				while (n < N && c > 0)
				{
					n++;
					P[0] = new ComplexVector(Row);
					Obj = f.Evaluate(P, v);
					Row = Obj.AssociatedObjectValue as Complex[];

					if (Row is null)
					{
						throw new ScriptRuntimeException("Lambda expression must be able to accept complex vectors, " +
							"and return complex vectors of equal length. Type returned: " +
							Obj.GetType().FullName, Node);
					}
					else if (Row.Length != c)
					{
						throw new ScriptRuntimeException("Lambda expression must be able to accept complex vectors, " +
							"and return complex vectors of equal length. Length returned: " +
							Row.Length.ToString() + ". Expected: " + c.ToString(), Node);
					}

					for (x = x2 = 0; x < c; x++)
					{
						z = Row[x];
						j = Offset[x];

						Mod = z.Magnitude;

						if (Mod < 3)
						{
							if (x != x2)
							{
								Row[x2] = z;
								Offset[x2] = j;
							}

							x2++;
						}
						else
						{
							rgb[j++] = 0;
							rgb[j++] = 0;
							rgb[j++] = 0;
							rgb[j++] = 255;
						}
					}

					if (x2 < x)
					{
						Array.Resize(ref Row, x2);
						Array.Resize(ref Offset, x2);
						c = x2;
					}
				}

				if (c > 0)
				{
					for (x = 0; x < c; x++)
					{
						z = Row[x];
						j = Offset[x];

						Mod = z.Magnitude;
						n = (int)(N * Mod / 3 + 0.5);

						if (n < N)
						{
							Counts[n]++;
							rgb[j++] = blues[n];
							rgb[j++] = greens[n];
							rgb[j++] = reds[n];
						}
						else
						{
							rgb[j++] = 0;
							rgb[j++] = 0;
							rgb[j++] = 0;
						}

						rgb[j++] = 255;
					}
				}

			}

			Variables.ConsoleOut.WriteLine(Expression.ToString(Counts));

			PixelInformation Pixels = new PixelInformationRaw(SKColorType.Bgra8888, rgb, Width, Height, Width << 2);
			return new FractalGraph(Pixels, r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(JuliaInternalFractal);
	}
}
