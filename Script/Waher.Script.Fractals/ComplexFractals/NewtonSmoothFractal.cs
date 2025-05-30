﻿using System;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Fractals.ComplexFractals
{
	/// <summary>
	/// Calculates a Newton Fractal Image, and smoothes the colors using
	/// the heat equation in two spatial dimensions and one time dimension.
	/// </summary>
	/// <remarks>
	/// http://en.wikipedia.org/wiki/Heat_equation
	/// </remarks>
	/// <example>
	/// NewtonSmoothFractal((0,0),3,,[-1,0,0,1])
	/// NewtonSmoothFractal((0,0),3,,[-1,0,1])
	/// NewtonSmoothFractal((0,0),3,,[-1,0,0,0,1])
	/// NewtonSmoothFractal((0,0),3,,[-1,0,0,0,0,1])
	/// NewtonSmoothFractal((0,0),3,,[-1,0,0,0,0,0,1])
	/// NewtonSmoothFractal((0,0),3,,[-1,0,0,0,0,0,0,1])
	/// NewtonSmoothFractal((0,0),5,,[2,-2,0,1])
	/// NewtonSmoothFractal((0,0),5,,[-16,0,0,0,15,0,0,0,1])
	/// NewtonSmoothFractal((0,0),10,,[-16,0,0,0,15,0,0,0,1])
	/// NewtonSmoothFractal((-0.0732421875,0.0557911132812496),0.0029296875,,[1, 0, 0, 4, 0, 0, 0, 15],,640,480)
	/// NewtonSmoothFractal((0,0),3,,Uniform(0,5,8),,640,480)
	/// NewtonSmoothFractal((0,0),3,,Uniform(0,5,8),RandomLinearRGB(1024,4,1325528060), 640,480)
	/// NewtonSmoothFractal((-0.25755615234375,-0.03313134765625),0.01171875,,[1.95727631540842, 2.08149014137755, 2.98667318559562, 1.07363583337219, 4.51716111251952, 1.92312554545846, 4.77838875482715, 3.5401921014023],RandomLinearRGB(128,8,1738551375),640,480)
	/// NewtonSmoothFractal((0,0),3,,[-1, 0, 0, 0, 0, 1],RandomLinearAnalogousHSL(128,4,746040511),2000,2000)
	/// NewtonSmoothFractal((0,0),3,1,,[-1, 0, 0, 0, 0, 1],RandomLinearAnalogousHSL(128,4,581646431),800,600)
	/// 
	/// Generalized Newton Fractals:
	/// NewtonSmoothFractal((0,0),3,0.5,[-1,0,0,1],,640,480)
	/// NewtonSmoothFractal((0,0),3,1.5,[-1,0,0,1],,640,480)
	/// NewtonSmoothFractal((0,0),3,0.5,Uniform(0,5,8),,640,480)
	/// NewtonSmoothFractal((0,0),3,1.5,Uniform(0,5,8),,640,480)
	/// NewtonSmoothFractal((-0.478125,-0.536125),0.75,(0.5,0.5),[-1, 0, 0, 0, i, 0, 0, 1],RandomLinearAnalogousHSL(1024,4,518754351),640,480)
	/// NewtonSmoothFractal((-0.4875,-0.519125),0.75,(0.5,0.5),[-1, 0, 0, 0, i, 0, 0, 1],RandomLinearAnalogousHSL(1024,4,2136836666),800,600)
	/// 
	/// Using Lambda expressions:
	/// NewtonSmoothFractal((pi/2,0),pi/4,,x->sin(x),RandomLinearRGB(128,16,1177860657),640,480)
	/// </example>
	public class NewtonSmoothFractal : FunctionMultiVariate
	{
		/// <summary>
		/// TODO
		/// </summary>
		public NewtonSmoothFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c, ScriptNode Palette,
			ScriptNode DimX, ScriptNode DimY, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R, c, Palette, DimX, DimY },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal, ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar},
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public NewtonSmoothFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c, ScriptNode Palette,
			ScriptNode DimX, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R, c, Palette, DimX },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal, ArgumentType.Vector, ArgumentType.Scalar},
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public NewtonSmoothFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c, ScriptNode Palette,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R, c, Palette },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal, ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public NewtonSmoothFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R, c },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "z", "dr", "R", "c", "Palette", "DimX", "DimY" };
			}
		}

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// TODO
		/// </summary>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			string ColorExpression = null;
			SKColor[] Palette;
			double[] Coefficients = null;
			Complex[] CoefficientsZ = null;
			ILambdaExpression f = null;
			ScriptNode fDef = null;
			double rc, ic;
			double dr;
			Complex R;
			int dimx, dimy;
			int c = Arguments.Length;
			int i = 0;
			object Obj;

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
				throw new ScriptRuntimeException("Insufficient parameters in call to NewtonSmoothFractal().", this);

			dr = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);

			if (i < c && ((Obj = Arguments[i].AssociatedObjectValue) is double || Obj is Complex))
			{
				R = Expression.ToComplex(Obj);
				i++;
			}
			else
			{
				R = Complex.One;

				if (i < c && this.Arguments[i] is null)
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
						throw new ScriptRuntimeException("Lambda expression in calls to NewtonSmoothFractal() must be of one variable.", this);

					fDef = this.Arguments[i++];
				}
				else
				{
					throw new ScriptRuntimeException("Parameter " + (i + 1).ToString() +
						" in call to NewtonSmoothFractal has to be a vector of numbers, containing coefficients " +
						"of the polynomial to use. Now it was of type " + Arguments[i].GetType().FullName,
						this);
				}
			}
			else
				throw new ScriptRuntimeException("Missing coefficients or lambda expression.", this);

			if (i < c && !(this.Arguments[i] is null) && Arguments[i] is ObjectVector)
			{
				ColorExpression = this.Arguments[i].SubExpression;
				Palette = FractalGraph.ToPalette((ObjectVector)Arguments[i++]);
			}
			else
			{
				Palette = ColorModels.RandomLinearAnalogousHSL.CreatePalette(128, 4, out int Seed, this, Variables);
				ColorExpression = "RandomLinearAnalogousHSL(128,4," + Seed.ToString() + ")";

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
				throw new ScriptRuntimeException("Parameter mismatch in call to NewtonSmoothFractal(r,c,dr,Coefficients[,Palette][,dimx[,dimy]]).",
					this);
			}

			if (dimx <= 0 || dimx > 5000 || dimy <= 0 || dimy > 5000)
				throw new ScriptRuntimeException("Image size must be within 1x1 to 5000x5000", this);

			if (!(f is null))
			{
				return await CalcNewtonSmooth(rc, ic, dr, R, f, Variables, Palette, dimx, dimy, this, this.FractalZoomScript,
					new object[] { Palette, dimx, dimy, R, fDef, ColorExpression });
			}
			else if (!(CoefficientsZ is null))
			{
				return await CalcNewtonSmooth(rc, ic, dr, R, CoefficientsZ, Palette, dimx, dimy,
					this, Variables, this.FractalZoomScript,
					new object[] { Palette, dimx, dimy, R, CoefficientsZ, ColorExpression });
			}
			else
			{
				return await CalcNewtonSmooth(rc, ic, dr, R, Coefficients, Palette, dimx, dimy,
					this, Variables, this.FractalZoomScript,
					new object[] { Palette, dimx, dimy, R, Coefficients, ColorExpression });
			}
		}

		private string FractalZoomScript(double r, double i, double Size, object State)
		{
			object[] Parameters = (object[])State;
			int DimX = (int)Parameters[1];
			int DimY = (int)Parameters[2];
			Complex R = (Complex)Parameters[3];
			double[] Coefficients = Parameters[4] as double[];
			string ColorExpression = (string)Parameters[5];

			StringBuilder sb = new StringBuilder();

			sb.Append("NewtonSmoothFractal((");
			sb.Append(Expression.ToString(r));
			sb.Append(',');
			sb.Append(Expression.ToString(i));
			sb.Append("),");
			sb.Append(Expression.ToString(Size / 4));
			sb.Append(',');
			sb.Append(Expression.ToString(R));
			sb.Append(',');

			if (Parameters[4] is ScriptNode fDef)
				sb.Append(fDef.SubExpression);
			else if (Parameters[4] is Complex[] CoefficientsZ)
				sb.Append(Expression.ToString(CoefficientsZ));
			else
				sb.Append(Expression.ToString(Coefficients));

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
		public static async Task<FractalGraph> CalcNewtonSmooth(double rCenter, double iCenter, double rDelta, Complex R,
			double[] Coefficients, SKColor[] Palette, int Width, int Height, ScriptNode Node, Variables Variables,
			FractalZoomScript FractalZoomScript, object State)
		{
			double RRe = R.Real;
			double RIm = R.Imaginary;
			double r0, i0, r1, i1;
			double dr, di;
			double r, i;
			double zr, zi, zr2, zi2, zr3, zi3, zr4, zi4;
			double aspect;
			double Temp;
			int x, y;
			int n, N;
			int Degree = Coefficients.Length - 1;

			N = Palette.Length;

			if (Degree < 2)
			{
				Array.Resize(ref Coefficients, 3);
				while (Degree < 2)
					Coefficients[++Degree] = 0;
			}

			if (Width <= 2 || Height <= 2)
				throw new ScriptRuntimeException("Width and Height has to be greater than 2.", Node);

			double[] Prim = new double[Degree];
			for (x = 1; x <= Degree; x++)
				Prim[x - 1] = x * Coefficients[x];

			Array.Reverse(Prim);
			Coefficients = (double[])Coefficients.Clone();
			Array.Reverse(Coefficients);

			int Size = Width * Height;
			double Conv = 1e-10;
			double Div = 1e10;
			double[] ColorIndex = new double[Size];
			int Index = 0;

			rDelta *= 0.5;
			r0 = rCenter - rDelta;
			r1 = rCenter + rDelta;

			aspect = ((double)Width) / Height;

			i0 = iCenter - rDelta / aspect;
			i1 = iCenter + rDelta / aspect;

			dr = (r1 - r0) / Width;
			di = (i1 - i0) / Height;

			for (y = 0, i = i0; y < Height; y++, i += di)
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
						ColorIndex[Index++] = n;
					else
						ColorIndex[Index++] = N;
				}
			}

			await Variables.Preview(Node.Expression, new GraphBitmap(Variables, FractalGraph.ToPixels(ColorIndex, Width, Height, Palette)));

			double[] Boundary = FractalGraph.FindBoundaries(ColorIndex, Width, Height);
			await FractalGraph.Smooth(ColorIndex, Boundary, Width, Height, N, Palette, Node, Variables);

			return new FractalGraph(Variables, FractalGraph.ToPixels(ColorIndex, Width, Height, Palette),
				r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public static async Task<FractalGraph> CalcNewtonSmooth(double rCenter, double iCenter, double rDelta, Complex R,
			Complex[] Coefficients, SKColor[] Palette, int Width, int Height, ScriptNode Node, Variables Variables,
			FractalZoomScript FractalZoomScript, object State)
		{
			double RRe = R.Real;
			double RIm = R.Imaginary;
			double r0, i0, r1, i1;
			double dr, di;
			double r, i;
			double zr, zi, zr2, zi2, zr3, zi3, zr4, zi4;
			double aspect;
			double Temp;
			int x, y;
			int n, N;
			int Degree = Coefficients.Length - 1;

			N = Palette.Length;

			if (Degree < 2)
			{
				Array.Resize(ref Coefficients, 3);
				while (Degree < 2)
					Coefficients[++Degree] = Complex.Zero;
			}

			if (Width <= 2 || Height <= 2)
				throw new ScriptRuntimeException("Width and Height has to be greater than 2.", Node);

			Complex[] Prim = new Complex[Degree];
			for (x = 1; x <= Degree; x++)
				Prim[x - 1] = x * Coefficients[x];

			Array.Reverse(Prim);
			Coefficients = (Complex[])Coefficients.Clone();
			Array.Reverse(Coefficients);

			int j, c = Prim.Length;
			double[] ReC = new double[c + 1];
			double[] ImC = new double[c + 1];
			double[] RePrim = new double[c];
			double[] ImPrim = new double[c];
			Complex z;

			for (j = 0; j < c; j++)
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

			int Size = Width * Height;
			double Conv = 1e-10;
			double Div = 1e10;
			double[] ColorIndex = new double[Size];
			int Index = 0;

			rDelta *= 0.5;
			r0 = rCenter - rDelta;
			r1 = rCenter + rDelta;

			aspect = ((double)Width) / Height;

			i0 = iCenter - rDelta / aspect;
			i1 = iCenter + rDelta / aspect;

			dr = (r1 - r0) / Width;
			di = (i1 - i0) / Height;

			for (y = 0, i = i0; y < Height; y++, i += di)
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
						for (j = 0; j <= c; j++)
						{
							Temp = zr2 * zr - zi2 * zi + ReC[j];
							zi2 = zr2 * zi + zi2 * zr + ImC[j];
							zr2 = Temp;
						}

						// f':
						zr3 = zi3 = 0;
						for (j = 0; j < c; j++)
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
						ColorIndex[Index++] = n;
					else
						ColorIndex[Index++] = N;
				}
			}

			await Variables.Preview(Node.Expression, new GraphBitmap(Variables, FractalGraph.ToPixels(ColorIndex, Width, Height, Palette)));

			double[] Boundary = FractalGraph.FindBoundaries(ColorIndex, Width, Height);
			await FractalGraph.Smooth(ColorIndex, Boundary, Width, Height, N, Palette, Node, Variables);

			return new FractalGraph(Variables, FractalGraph.ToPixels(ColorIndex, Width, Height, Palette),
				r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public static async Task<FractalGraph> CalcNewtonSmooth(double rCenter, double iCenter, double rDelta, Complex R,
			ILambdaExpression f, Variables Variables, SKColor[] Palette, int Width, int Height,
			ScriptNode Node, FractalZoomScript FractalZoomScript, object State)
		{
			double RRe = R.Real;
			double RIm = R.Imaginary;
			double r0, i0, r1, i1;
			double dr, di;
			double r, i;
			double aspect;
			int x, y;
			int n, N;

			N = Palette.Length;

			if (Width <= 2 || Height <= 2)
				throw new ScriptRuntimeException("Width and Height has to be greater than 2.", Node);

			Variables v = new Variables();
			Variables.CopyTo(v);

			if (!(f is IDifferentiable Differentiable) ||
				!(Differentiable.Differentiate(Differentiable.DefaultVariableName, v) is ILambdaExpression fPrim))
			{
				throw new ScriptRuntimeException("Lambda expression not differentiable.", Node);
			}

			int Size = Width * Height;
			double Conv = 1e-10;
			double Div = 1e10;
			double[] ColorIndex = new double[Size];
			int Index = 0;

			Complex[] Row;
			Complex[] Row2;
			Complex[] Row3;
			int[] Offset;
			IElement[] P = new IElement[1];
			int j, c, x2;
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

			for (y = 0, i = i0; y < Height; y++, i += di)
			{
				Row = new Complex[Width];
				Offset = new int[Width];

				c = Width;
				for (x = 0, x2 = y * Width, r = r0; x < Width; x++, r += dr, x2++)
				{
					Row[x] = new Complex(r, i);
					Offset[x] = x2;
				}

				n = 0;
				while (n < N && c > 0)
				{
					n++;
					P[0] = new ComplexVector(Row);
					Obj = f.Evaluate(P, v);
					Obj2 = fPrim.Evaluate(P, v);
					Row2 = Obj.AssociatedObjectValue as Complex[];
					Row3 = Obj2.AssociatedObjectValue as Complex[];

					if (Row2 is null || Row3 is null)
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
								ColorIndex[Index++] = N;
							else
								ColorIndex[j++] = n;
						}
					}

					if (x2 < x)
					{
						Array.Resize(ref Row, x2);
						Array.Resize(ref Offset, x2);
						c = x2;
					}
				}
			}

			await Variables.Preview(Node.Expression, new GraphBitmap(Variables, FractalGraph.ToPixels(ColorIndex, Width, Height, Palette)));

			double[] Boundary = FractalGraph.FindBoundaries(ColorIndex, Width, Height);
			await FractalGraph.Smooth(ColorIndex, Boundary, Width, Height, N, Palette, Node, Variables);

			return new FractalGraph(Variables, FractalGraph.ToPixels(ColorIndex, Width, Height, Palette),
				r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(NewtonSmoothFractal);
	}
}
