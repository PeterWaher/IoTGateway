using System;
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
    /// Calculates a Mandelbrot Topography Fractal Image
    /// </summary>
    /// <example>
    /// MandelbrotTopographyFractal((-1,0),,3,RandomLinearRgb(1024,16,1),800,600)
    /// MandelbrotTopographyFractal((-0.65,-0.4125),,0.001,RandomLinearRGB(4096,128))
    /// MandelbrotTopographyFractal((-0.728474426269531,-0.240391845703126),,0.000732421875,RandomLinearRGB(4096,128,1325528060),640,480)
    /// MandelbrotTopographyFractal((-0.797603011131287,-0.179388891220092),,1.1444091796875E-05,randomlinearrgb(10240,16,1),320,200)
    /// 
    /// General version:
    /// MandelbrotTopographyFractal((0,0),(z,c)->z^3+c,4,RandomLinearAnalogousHsl(1024,16,21),400,400)
    /// </example>
    public class MandelbrotTopographyFractal : FunctionMultiVariate
    {
		public MandelbrotTopographyFractal(ScriptNode z, ScriptNode f, ScriptNode dr, ScriptNode Palette,
			ScriptNode DimX, ScriptNode DimY, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { z, f, dr, Palette, DimX, DimY }, 
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar}, Start, Length, Expression)
        {
		}

		public MandelbrotTopographyFractal(ScriptNode z, ScriptNode f, ScriptNode dr, ScriptNode Palette,
			ScriptNode DimX, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, f, dr, Palette, DimX },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Vector, ArgumentType.Scalar}, Start, Length, Expression)
		{
		}

		public MandelbrotTopographyFractal(ScriptNode z, ScriptNode f, ScriptNode dr, ScriptNode Palette, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, f, dr, Palette },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Vector}, Start, Length, Expression)
		{
		}

		public MandelbrotTopographyFractal(ScriptNode z, ScriptNode f, ScriptNode dr, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, f, dr },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "z", "f", "dr", "Palette", "DimX", "DimY" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            string ColorExpression = null;
            SKColor[] Palette;
            double rc, ic;
            double dr;
            int dimx, dimy;
            int i, c;
            object Obj;
            Complex z;
            ILambdaExpression f;
            ScriptNode fDef = null;
            c = Arguments.Length;
            i = 0;

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
                throw new ScriptRuntimeException("Insufficient parameters in call to MandelbrotTopographyFractal().", this);

            Obj = Arguments[i].AssociatedObjectValue;
            if (Obj is ILambdaExpression)
            {
                f = (ILambdaExpression)Obj;
                fDef = this.Arguments[i++];

                if (f.NrArguments != 2)
                    throw new ScriptRuntimeException("Lambda expression in calls to MandelbrotTopographyFractal() must be of two variables (z,c).", this);
            }
            else
            {
                f = null;
                fDef = null;

				if (Obj == null)
					i++;
			}

			dr = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);

			if (i < c && this.Arguments[i] != null && Arguments[i] is ObjectVector)
			{
				ColorExpression = this.Arguments[i].SubExpression;
                Palette = FractalGraph.ToPalette((ObjectVector)Arguments[i++]);
            }
            else
            {
                Palette = ColorModels.RandomLinearAnalogousHSL.CreatePalette(1024, 16, out int Seed, this, Variables);
                ColorExpression = "RandomLinearAnalogousHSL(1024,16," + Seed.ToString() + ")";

				if (i < c && this.Arguments[i] == null)
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
                throw new ScriptRuntimeException("Parameter mismatch in call to MandelbrotTopographyFractal(r,c,dr[,Palette][,dimx[,dimy]]).",
                    this);
            }

            if (dimx <= 0 || dimx > 5000 || dimy <= 0 || dimy > 5000)
                throw new ScriptRuntimeException("Image size must be within 1x1 to 5000x5000", this);

            if (f != null)
            {
                return CalcMandelbrot(rc, ic, dr, f, Variables, Palette, dimx, dimy, this, this.FractalZoomScript,
                    new object[] { Palette, dimx, dimy, ColorExpression, fDef });
            }
            else
            {
                return CalcMandelbrot(rc, ic, dr, Palette, dimx, dimy, this, this.FractalZoomScript,
                    new object[] { Palette, dimx, dimy, ColorExpression, fDef });
            }
        }

        private string FractalZoomScript(double r, double i, double Size, object State)
        {
            object[] Parameters = (object[])State;
            SKColor[] Palette = (SKColor[])Parameters[0];
            int DimX = (int)Parameters[1];
            int DimY = (int)Parameters[2];
            string ColorExpression = (string)Parameters[3];
            ScriptNode f = (ScriptNode)Parameters[4];

            StringBuilder sb = new StringBuilder();

            sb.Append("MandelbrotTopographyFractal(");
            sb.Append(Expression.ToString(r));
            sb.Append(",");
            sb.Append(Expression.ToString(i));

            if (f != null)
            {
                sb.Append(",");
                sb.Append(f.SubExpression);
            }

            sb.Append(",");
            sb.Append(Expression.ToString(Size / 4));

            if (!string.IsNullOrEmpty(ColorExpression))
            {
                sb.Append(",");
                sb.Append(ColorExpression);
            }

            sb.Append(",");
            sb.Append(DimX.ToString());
            sb.Append(",");
            sb.Append(DimY.ToString());
            sb.Append(")");

            return sb.ToString();
        }

        public static FractalGraph CalcMandelbrot(double rCenter, double iCenter, double rDelta,
            SKColor[] Palette, int Width, int Height, ScriptNode Node,
            FractalZoomScript FractalZoomScript, object State)
        {
            double r0, i0, r1, i1;
            double dr, di;
            double r, i;
            double zr, zi, zrt, zr2, zi2;
            double aspect;
            int x, y;
            int n, N;

            N = Palette.Length;

            int Size = Width * Height;
            int[] ColorIndex = new int[Size];
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
                    zr2 = zr * zr;
                    zi2 = zi * zi;

                    while (zr2 + zi2 < 9 && n < N)
                    {
                        n++;
                        zrt = zr2 - zi2 + r;
                        zi = 2 * zr * zi + i;
                        zr = zrt;

                        zr2 = zr * zr;
                        zi2 = zi * zi;
                    }

                    if (n >= N)
                        ColorIndex[Index++] = N;
                    else
                        ColorIndex[Index++] = n;
                }
            }

            ColorIndex = FractalGraph.FindBoundaries(ColorIndex, Width, Height);

            return new FractalGraph(FractalGraph.ToBitmap(ColorIndex, Width, Height, Palette), 
                r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
        }

        public static FractalGraph CalcMandelbrot(double rCenter, double iCenter, double rDelta,
            ILambdaExpression f, Variables Variables, SKColor[] Palette, int Width, int Height,
            ScriptNode Node, FractalZoomScript FractalZoomScript, object State)
        {
            double r0, i0, r1, i1;
            double dr, di;
            double r, i, Mod;
            double aspect;
            int x, x2, y;
            int n, N;

            N = Palette.Length;

            int Size = Width * Height;
            int[] ColorIndex = new int[Size];

            Complex z;
            IElement[] P = new IElement[2];
            int j, c;
            IElement Obj;

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
                Complex[] Row = new Complex[Width];
                Complex[] Row0 = new Complex[Width];
                int[] Offset = new int[Width];

                c = Width;
                for (x = 0, x2 = y * Width, r = r0; x < Width; x++, r += dr, x2++)
                {
                    Row[x] = Row0[x] = new Complex(r, i);
                    Offset[x] = x2;
                }

                Variables v = new Variables();
                Variables.CopyTo(v);

                n = 0;
                while (n < N && c > 0)
                {
                    n++;
                    P[0] = Expression.Encapsulate(Row);
                    P[1] = Expression.Encapsulate(Row0);
                    Obj = f.Evaluate(P, v);
                    Row = Obj.AssociatedObjectValue as Complex[];

                    if (Row == null)
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
                                Row0[x2] = Row0[x];
                                Offset[x2] = j;
                            }

                            x2++;
                        }
                        else
                        {
                            if (n >= N)
                                ColorIndex[j++] = N;
                            else
                                ColorIndex[j++] = n;
                        }
                    }

                    if (x2 < x)
                    {
                        Array.Resize<Complex>(ref Row, x2);
                        Array.Resize<Complex>(ref Row0, x2);
                        Array.Resize<int>(ref Offset, x2);
                        c = x2;
                    }
                }

                if (c > 0)
                {
                    for (x = 0; x < c; x++)
                    {
                        j = Offset[x];
                        ColorIndex[j] = N;
                    }
                }

            }

            ColorIndex = FractalGraph.FindBoundaries(ColorIndex, Width, Height);

            return new FractalGraph(FractalGraph.ToBitmap(ColorIndex, Width, Height, Palette),
                r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
        }

        public override string FunctionName
        {
            get { return "MandelbrotTopographyFractal"; }
        }

    }
}