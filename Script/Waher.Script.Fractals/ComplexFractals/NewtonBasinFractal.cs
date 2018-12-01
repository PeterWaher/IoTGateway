using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Exceptions;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Fractals.ComplexFractals
{
	/// <summary>
	/// Calculates a Newton Basin Fractal Image
	/// </summary>
	/// <example>
	/// NewtonBasinFractal((0,0),3,,[-1,0,1])
	/// NewtonBasinFractal((0,0),3,,[-1,0,0,1])
	/// NewtonBasinFractal((0,0),3,,[-1,0,0,0,1])
	/// NewtonBasinFractal((0,0),3,,[-1,0,0,0,0,1],48)
	/// NewtonBasinFractal((0,0),3,,[-1,0,0,0,0,0,1],48)
	/// NewtonBasinFractal((0,0),3,,[-1,0,0,0,0,0,0,1],64)
	/// NewtonBasinFractal((0,0),5,,[2,-2,0,1])
	/// NewtonBasinFractal((0,0),5,,[-16,0,0,0,15,0,0,0,1])
	/// NewtonBasinFractal((0,0),10,,[-16,0,0,0,15,0,0,0,1])
	/// NewtonBasinFractal((0,0),5,,Uniform(0,5,8),32,640,480)
	/// 
	/// Generalized Newton Fractals:
	/// NewtonBasinFractal((0,0),3,0.5,[-1,0,0,1],32,640,480)
	/// NewtonBasinFractal((0,0),3,1.5,[-1,0,0,1],32,640,480)
	/// NewtonBasinFractal((0,0),3,0.5,Uniform(0,5,8),32,640,480)
	/// NewtonBasinFractal((0,0),3,1.5,Uniform(0,5,8),32,640,480)
	/// 
	/// Using Lambda expressions:
	/// NewtonBasinFractal((pi/2,0),pi/4,,x->sin(x),32,640,480)
	/// </example>
	public class NewtonBasinFractal : FunctionMultiVariate
	{
		public NewtonBasinFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c, ScriptNode N,
			ScriptNode DimX, ScriptNode DimY, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R, c, N, DimX, DimY },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar},
				  Start, Length, Expression)
		{
		}

		public NewtonBasinFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c, ScriptNode N,
			ScriptNode DimX, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R, c, N, DimX },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal, ArgumentType.Scalar, ArgumentType.Scalar},
				  Start, Length, Expression)
		{
		}

		public NewtonBasinFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c, ScriptNode N,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R, c, N },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		public NewtonBasinFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R, c },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "z", "dr", "R", "c", "N", "DimX", "DimY" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			double[] Coefficients = null;
			Complex[] CoefficientsZ = null;
			ILambdaExpression f = null;
			ScriptNode fDef = null;
			double rc, ic;
			double dr;
			Complex R;
			int N;
			int dimx, dimy;
			int c = Arguments.Length;
			int i = 0;
			object Obj;
			Complex z;

			Obj = Arguments[i++].AssociatedObjectValue;
			if (Obj is Complex)
			{
				z = (Complex)Obj;
				rc = z.Real;
				ic = z.Imaginary;
			}
			else
			{
				rc = Expression.ToDouble(Obj);
				ic = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			}

			if (i >= c)
				throw new ScriptRuntimeException("Insufficient parameters in call to NewtonBasinFractal().", this);

			dr = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);

			if (i < c && (Arguments[i] is DoubleNumber || Arguments[i] is ComplexNumber))
				R = Expression.ToComplex(Arguments[i++].AssociatedObjectValue);
			else
			{
				R = Complex.One;

				if (i < c && this.Arguments[i] == null)
					i++;
			}

			if (i < c)
			{
				if (Arguments[i] is DoubleVector)
					Coefficients = (double[])Arguments[i++].AssociatedObjectValue;
				else if (Arguments[i] is ComplexVector)
					CoefficientsZ = (Complex[])Arguments[i++].AssociatedObjectValue;
				/*else if (Parameters[i] is RealPolynomial)
					Coefficients = ((RealPolynomial)Arguments[i++].AssociatedObjectValue).Coefficients;
				else if (Parameters[i] is ComplexPolynomial)
					CoefficientsZ = ((ComplexPolynomial)Arguments[i++].AssociatedObjectValue).Coefficients;*/
				else if (Arguments[i] is IVector)
				{
					IVector Vector = (IVector)Arguments[i++];
					int j, d = Vector.Dimension;

					CoefficientsZ = new Complex[d];
					for (j = 0; j < d; j++)
						CoefficientsZ[j] = Expression.ToComplex(Vector.GetElement(j).AssociatedObjectValue);
				}
				else if (Arguments[i].AssociatedObjectValue is ILambdaExpression)
				{
					f = (ILambdaExpression)Arguments[i];
					if (f.NrArguments != 1)
						throw new ScriptRuntimeException("Lambda expression in calls to NewtonBasinFractal() must be of one variable.", this);

					fDef = this.Arguments[i++];
				}
				else
				{
					throw new ScriptRuntimeException("Parameter " + (i + 1).ToString() +
						" in call to NewtonBasinFractal has to be a vector of numbers, containing coefficients " +
						"of the polynomial to use. Now it was of type " + Arguments[i].GetType().FullName,
						this);
				}
			}
			else
				throw new ScriptRuntimeException("Missing coefficients or lambda expression.", this);

			if (i < c)
				N = (int)Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			else
				N = 32;

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
				throw new ScriptRuntimeException("Parameter mismatch in call to NewtonBasinFractal(r,c,dr,Coefficients[,Palette][,dimx[,dimy]]).",
					this);
			}

			if (dimx <= 0 || dimx > 5000 || dimy <= 0 || dimy > 5000)
				throw new ScriptRuntimeException("Image size must be within 1x1 to 5000x5000", this);

			if (f != null)
			{
				return CalcNewton(rc, ic, dr, R, f, fDef, Variables, N, dimx, dimy,
					this, this.FractalZoomScript,
					new object[] { dimx, dimy, N, R, fDef });
			}
			else if (CoefficientsZ != null)
			{
				return CalcNewton(rc, ic, dr, R, CoefficientsZ, N, dimx, dimy,
					this, this.FractalZoomScript,
					new object[] { dimx, dimy, N, R, CoefficientsZ });
			}
			else
			{
				return CalcNewton(rc, ic, dr, R, Coefficients, N, dimx, dimy,
					this, this.FractalZoomScript,
					new object[] { dimx, dimy, N, R, Coefficients });
			}
		}

		private string FractalZoomScript(double r, double i, double Size, object State)
		{
			object[] Parameters = (object[])State;
			int DimX = (int)Parameters[0];
			int DimY = (int)Parameters[1];
			int N = (int)Parameters[2];
			Complex R = (Complex)Parameters[3];
			double[] Coefficients = Parameters[4] as double[];
			Complex[] CoefficientsZ = Parameters[4] as Complex[];

			StringBuilder sb = new StringBuilder();

			sb.Append("NewtonBasinFractal(");
			sb.Append(Expression.ToString(r));
			sb.Append(",");
			sb.Append(Expression.ToString(i));
			sb.Append(",");
			sb.Append(Expression.ToString(Size / 4));
			sb.Append(",");
			sb.Append(Expression.ToString(R));
			sb.Append(",");

			if (Parameters[4] is ScriptNode fDef)
				sb.Append(fDef.SubExpression);
			else if (CoefficientsZ != null)
				sb.Append(Expression.ToString(CoefficientsZ));
			else
				sb.Append(Expression.ToString(Coefficients));

			sb.Append(",");
			sb.Append(N.ToString());
			sb.Append(",");
			sb.Append(DimX.ToString());
			sb.Append(",");
			sb.Append(DimY.ToString());
			sb.Append(")");

			return sb.ToString();
		}

		public static FractalGraph CalcNewton(double rCenter, double iCenter, double rDelta, Complex R,
			double[] Coefficients, int N, int Width, int Height, ScriptNode Node,
			FractalZoomScript FractalZoomScript, object State)
		{
			double RRe = R.Real;
			double RIm = R.Imaginary;
			List<double> AttractorsR = new List<double>();
			List<double> AttractorsI = new List<double>();
			List<int> AttractorColors = new List<int>();
			double[] AttractorsR2 = new double[0];
			double[] AttractorsI2 = new double[0];
			int[] AttractorsColors2 = new int[0];
			double r0, i0, r1, i1;
			double dr, di;
			double r, i;
			double zr, zi, zr2, zi2, zr3, zi3, zr4, zi4;
			double aspect;
			double Temp;
			int x, y, b, c = 0, d;
			int n;
			int index;
			int Degree = Coefficients.Length - 1;

			if (Degree < 2)
			{
				Array.Resize<double>(ref Coefficients, 3);
				while (Degree < 2)
					Coefficients[++Degree] = 0;
			}

			double[] Prim = new double[Degree];
			for (x = 1; x <= Degree; x++)
				Prim[x - 1] = x * Coefficients[x];

			Array.Reverse(Prim);
			Coefficients = (double[])Coefficients.Clone();
			Array.Reverse(Coefficients);

			int size = Width * Height * 4;
			double Conv = 1e-10;
			double Div = 1e10;
			double Conv2 = Conv * 2;
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
					do
					{
						// f:
						zr2 = zi2 = 0;
						foreach (double C in Coefficients)
						{
							Temp = zr2 * zr - zi2 * zi + C;
							zi2 = zr2 * zi + zi2 * zr;
							zr2 = Temp;
						}

						// f':
						zr3 = zi3 = 0;
						foreach (double C in Prim)
						{
							Temp = zr3 * zr - zi3 * zi + C;
							zi3 = zr3 * zi + zi3 * zr;
							zr3 = Temp;
						}

						// f/f':

						Temp = 1.0 / (zr3 * zr3 + zi3 * zi3);
						zr4 = (zr2 * zr3 + zi2 * zi3) * Temp;
						zi4 = (zi2 * zr3 - zr2 * zi3) * Temp;

						// R*f/f'
						Temp = zr4 * RRe - zi4 * RIm;
						zi4 = zr4 * RIm + zi4 * RRe;
						zr4 = Temp;

						zr -= zr4;
						zi -= zi4;

						Temp = Math.Sqrt(zr4 * zr4 + zi4 * zi4);
					}
					while ((Temp > Conv) && (Temp < Div) && (n++ < N));

					if (Temp < Conv && n < N)
					{
						for (b = 0; b < c; b++)
						{
							if (Math.Abs(AttractorsR2[b] - zr) < Conv2 &&
								Math.Abs(AttractorsI2[b] - zi) < Conv2)
							{
								break;
							}
						}

						if (b == c)
						{
							AttractorsR.Add(zr);
							AttractorsI.Add(zi);

							int p1 = ~((b % 6) + 1);
							int p2 = ((b / 6) % 7);

							int Red = (p1 & 1) != 0 ? 255 : 0;
							int Green = (p1 & 2) != 0 ? 255 : 0;
							int Blue = (p1 & 4) != 0 ? 255 : 0;

							if ((p2 & 1) != 0)
								Red >>= 1;

							if ((p2 & 2) != 0)
								Green >>= 1;

							if ((p2 & 4) != 0)
								Blue >>= 1;

							Blue <<= 8;
							Blue |= Green;
							Blue <<= 8;
							Blue |= Red;

							AttractorColors.Add(Blue);

							AttractorsR2 = AttractorsR.ToArray();
							AttractorsI2 = AttractorsI.ToArray();
							AttractorsColors2 = AttractorColors.ToArray();

							c++;
						}

						b = AttractorColors[b];

						d = (byte)b;
						rgb[index++] = (byte)((d * (N - n + 1)) / N);
						b >>= 8;
						d = (byte)b;
						rgb[index++] = (byte)((d * (N - n + 1)) / N);
						b >>= 8;
						d = (byte)b;
						rgb[index++] = (byte)((d * (N - n + 1)) / N);
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

			using (SKData Data = SKData.Create(new MemoryStream(rgb)))
			{
				SKImage Bitmap = SKImage.FromPixelData(new SKImageInfo(Width, Height, SKColorType.Bgra8888), Data, Width * 4);
				return new FractalGraph(Bitmap, r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
			}
		}

		public static FractalGraph CalcNewton(double rCenter, double iCenter, double rDelta, Complex R,
			Complex[] Coefficients, int N, int Width, int Height, ScriptNode Node,
			FractalZoomScript FractalZoomScript, object State)
		{
			double RRe = R.Real;
			double RIm = R.Imaginary;
			List<double> AttractorsR = new List<double>();
			List<double> AttractorsI = new List<double>();
			List<int> AttractorColors = new List<int>();
			double[] AttractorsR2 = new double[0];
			double[] AttractorsI2 = new double[0];
			int[] AttractorsColors2 = new int[0];
			double r0, i0, r1, i1;
			double dr, di;
			double r, i;
			double zr, zi, zr2, zi2, zr3, zi3, zr4, zi4;
			double aspect;
			double Temp;
			int x, y, b, c = 0, d;
			int n;
			int index;
			int Degree = Coefficients.Length - 1;

			if (Degree < 2)
			{
				Array.Resize<Complex>(ref Coefficients, 3);
				while (Degree < 2)
					Coefficients[++Degree] = Complex.Zero;
			}

			Complex[] Prim = new Complex[Degree];
			for (x = 1; x <= Degree; x++)
				Prim[x - 1] = x * Coefficients[x];

			Array.Reverse(Prim);
			Coefficients = (Complex[])Coefficients.Clone();
			Array.Reverse(Coefficients);

			int j, e = Prim.Length;
			double[] ReC = new double[e + 1];
			double[] ImC = new double[e + 1];
			double[] RePrim = new double[e];
			double[] ImPrim = new double[e];
			Complex z;

			for (j = 0; j < e; j++)
			{
				z = Coefficients[j];
				ReC[j] = z.Real;
				ImC[j] = z.Imaginary;

				z = Prim[j];
				RePrim[j] = z.Real;
				ImPrim[j] = z.Imaginary;
			}

			z = Coefficients[j];
			ReC[j] = z.Real;
			ImC[j] = z.Imaginary;

			int size = Width * Height * 4;
			double Conv = 1e-10;
			double Div = 1e10;
			double Conv2 = Conv * 2;
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
					do
					{
						// f:
						zr2 = zi2 = 0;
						for (j = 0; j <= e; j++)
						{
							Temp = zr2 * zr - zi2 * zi + ReC[j];
							zi2 = zr2 * zi + zi2 * zr + ImC[j];
							zr2 = Temp;
						}

						// f':
						zr3 = zi3 = 0;
						for (j = 0; j < e; j++)
						{
							Temp = zr3 * zr - zi3 * zi + RePrim[j];
							zi3 = zr3 * zi + zi3 * zr + ImPrim[j];
							zr3 = Temp;
						}

						// f/f':

						Temp = 1.0 / (zr3 * zr3 + zi3 * zi3);
						zr4 = (zr2 * zr3 + zi2 * zi3) * Temp;
						zi4 = (zi2 * zr3 - zr2 * zi3) * Temp;

						// R*f/f'
						Temp = zr4 * RRe - zi4 * RIm;
						zi4 = zr4 * RIm + zi4 * RRe;
						zr4 = Temp;

						zr -= zr4;
						zi -= zi4;

						Temp = Math.Sqrt(zr4 * zr4 + zi4 * zi4);
					}
					while ((Temp > Conv) && (Temp < Div) && (n++ < N));

					if (Temp < Conv && n < N)
					{
						for (b = 0; b < c; b++)
						{
							if (Math.Abs(AttractorsR2[b] - zr) < Conv2 &&
								Math.Abs(AttractorsI2[b] - zi) < Conv2)
							{
								break;
							}
						}

						if (b == c)
						{
							AttractorsR.Add(zr);
							AttractorsI.Add(zi);

							int p1 = ~((b % 6) + 1);
							int p2 = ((b / 6) % 7);

							int Red = (p1 & 1) != 0 ? 255 : 0;
							int Green = (p1 & 2) != 0 ? 255 : 0;
							int Blue = (p1 & 4) != 0 ? 255 : 0;

							if ((p2 & 1) != 0)
								Red >>= 1;

							if ((p2 & 2) != 0)
								Green >>= 1;

							if ((p2 & 4) != 0)
								Blue >>= 1;

							Blue <<= 8;
							Blue |= Green;
							Blue <<= 8;
							Blue |= Red;

							AttractorColors.Add(Blue);

							AttractorsR2 = AttractorsR.ToArray();
							AttractorsI2 = AttractorsI.ToArray();
							AttractorsColors2 = AttractorColors.ToArray();

							c++;
						}

						b = AttractorColors[b];

						d = (byte)b;
						rgb[index++] = (byte)((d * (N - n + 1)) / N);
						b >>= 8;
						d = (byte)b;
						rgb[index++] = (byte)((d * (N - n + 1)) / N);
						b >>= 8;
						d = (byte)b;
						rgb[index++] = (byte)((d * (N - n + 1)) / N);
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

			using (SKData Data = SKData.Create(new MemoryStream(rgb)))
			{
				SKImage Bitmap = SKImage.FromPixelData(new SKImageInfo(Width, Height, SKColorType.Bgra8888), Data, Width * 4);
				return new FractalGraph(Bitmap, r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
			}
		}

		public static FractalGraph CalcNewton(double rCenter, double iCenter, double rDelta, Complex R,
			ILambdaExpression f, ScriptNode fDef, Variables Variables, int N, int Width, int Height,
			ScriptNode Node, FractalZoomScript FractalZoomScript, object State)
		{
			double RRe = R.Real;
			double RIm = R.Imaginary;
			List<double> AttractorsR = new List<double>();
			List<double> AttractorsI = new List<double>();
			List<int> AttractorColors = new List<int>();
			double[] AttractorsR2 = new double[0];
			double[] AttractorsI2 = new double[0];
			int[] AttractorsColors2 = new int[0];
			double r0, i0, r1, i1;
			double dr, di;
			double r, i;
			double zr, zi;
			double aspect;
			int x, y, b, c, d;
			int NrAttractors = 0;
			int n;
			int index;

			Variables v = new Variables();
			Variables.CopyTo(v);

			if (!(f is IDifferentiable Differentiable) ||
				!(Differentiable.Differentiate(Differentiable.DefaultVariableName, v) is ILambdaExpression fPrim))
			{
				throw new ScriptRuntimeException("Lambda expression not differentiable.", Node);
			}

			int size = Width * Height * 4;
			double Conv = 1e-10;
			double Div = 1e10;
			double Conv2 = Conv * 2;
			byte[] rgb = new byte[size];

			Complex[] Row;
			Complex[] Row2;
			Complex[] Row3;
			int[] Offset;
			IElement[] P = new IElement[1];
			int j, x2;
			IElement Obj, Obj2;
			double Mod;
			Complex z;

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
				Row = new Complex[Width];
				Offset = new int[Width];

				c = Width;
				for (x = 0, x2 = y * Width * 4, r = r0; x < Width; x++, r += dr, x2 += 4)
				{
					Row[x] = new Complex(r, i);
					Offset[x] = x2;
				}

				n = 0;
				while (n < N && c > 0)
				{
					n++;
					P[0] = Expression.Encapsulate(Row);
					Obj = f.Evaluate(P, v);
					Obj2 = fPrim.Evaluate(P, v);
					Row2 = Obj.AssociatedObjectValue as Complex[];
					Row3 = Obj2.AssociatedObjectValue as Complex[];

					if (Row2 == null || Row3 == null)
					{
						throw new ScriptRuntimeException("Lambda expression (and its first derivative) must be able to accept complex vectors, " +
							"and return complex vectors of equal length. Type returned: " +
							Obj.GetType().FullName + " and " + Obj2.GetType().FullName, Node);
					}
					else if (Row2.Length != c || Row3.Length != c)
					{
						throw new ScriptRuntimeException("Lambda expression (and its first derivative) must be able to accept complex vectors, " +
							"and return complex vectors of equal length. Length returned: " +
							Row2.Length.ToString() + " and " + Row3.Length.ToString() +
							". Expected: " + c.ToString(), Node);
					}

					for (x = x2 = 0; x < c; x++)
					{
						j = Offset[x];
						z = R * Row2[x] / Row3[x];
						Row[x] -= z;

						Mod = z.Magnitude;

						if (Mod > Conv && Mod < Div)
						{
							if (x != x2)
								Offset[x2] = j;

							x2++;
						}
						else
						{
							if (n >= N)
							{
								rgb[j++] = 0;
								rgb[j++] = 0;
								rgb[j++] = 0;
							}
							else
							{
								zr = z.Real;
								zi = z.Imaginary;

								for (b = 0; b < NrAttractors; b++)
								{
									if (Math.Abs(AttractorsR2[b] - zr) < Conv2 &&
										Math.Abs(AttractorsI2[b] - zi) < Conv2)
									{
										break;
									}
								}

								if (b == NrAttractors)
								{
									AttractorsR.Add(zr);
									AttractorsI.Add(zi);

									int p1 = ~((b % 6) + 1);
									int p2 = ((b / 6) % 7);

									int Red = (p1 & 1) != 0 ? 255 : 0;
									int Green = (p1 & 2) != 0 ? 255 : 0;
									int Blue = (p1 & 4) != 0 ? 255 : 0;

									if ((p2 & 1) != 0)
										Red >>= 1;

									if ((p2 & 2) != 0)
										Green >>= 1;

									if ((p2 & 4) != 0)
										Blue >>= 1;

									Blue <<= 8;
									Blue |= Green;
									Blue <<= 8;
									Blue |= Red;

									AttractorColors.Add(Blue);

									AttractorsR2 = AttractorsR.ToArray();
									AttractorsI2 = AttractorsI.ToArray();
									AttractorsColors2 = AttractorColors.ToArray();

									NrAttractors++;
								}

								b = AttractorColors[b];

								d = (byte)b;
								rgb[index++] = (byte)((d * (N - n + 1)) / N);
								b >>= 8;
								d = (byte)b;
								rgb[index++] = (byte)((d * (N - n + 1)) / N);
								b >>= 8;
								d = (byte)b;
								rgb[index++] = (byte)((d * (N - n + 1)) / N);
							}

							rgb[j++] = 255;
						}
					}

					if (x2 < x)
					{
						Array.Resize<Complex>(ref Row, x2);
						Array.Resize<int>(ref Offset, x2);
						c = x2;
					}
				}

			}

			using (SKData Data = SKData.Create(new MemoryStream(rgb)))
			{
				SKImage Bitmap = SKImage.FromPixelData(new SKImageInfo(Width, Height, SKColorType.Bgra8888), Data, Width * 4);
				return new FractalGraph(Bitmap, r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
			}
		}

		public override string FunctionName
		{
			get { return "NewtonBasinFractal"; }
		}
	}
}
