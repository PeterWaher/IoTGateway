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
	/// Calculates a Julia set Fractal Image
	/// </summary>
	/// <example>
	/// JuliaTopographyFractal(z,c,dr[,Colors][,DimX[,DimY]])
	/// JuliaTopographyFractal(z,f(z),dr[,Colors][,DimX[,DimY]])
	/// 
	/// JuliaTopographyFractal((0,0),(-0.65,-0.4125),3,RandomLinearAnalogousHSL(4096,128))
	/// JuliaTopographyFractal((0,0),(-0.785028076171875,-0.1465322265625),3,RandomLinearAnalogousHSL(1024,16,2056656298),640,480)
	/// 
	/// General version:
	/// JuliaTopographyFractal((0,0),z->z^2+i,3,RandomLinearAnalogousHsl(1024,16,21))
	/// JuliaTopographyFractal((0,0),z->sin(z),3,RandomLinearAnalogousHsl(1024,16,21))
	/// JuliaTopographyFractal((0,0),z->(1,0.1)*sin(z),7,RandomLinearAnalogousHsl(1024,16,21))
	/// JuliaTopographyFractal((0,0),z->(1,0.2)*sin(z),7,RandomLinearAnalogousHsl(1024,16,21))
	/// JuliaTopographyFractal((0,0),z->(1,0.3)*sin(z),7,RandomLinearAnalogousHsl(1024,16,21))
	/// JuliaTopographyFractal((0,0),z->(1,0.4)*sin(z),7,RandomLinearAnalogousHsl(1024,16,21))
	/// JuliaTopographyFractal((0,0),z->(1,0.5)*sin(z),7,RandomLinearAnalogousHsl(1024,16,21))
	/// 
	/// Inzoomningar:
	/// JuliaTopographyFractal((0.0437500000000002,0.00481249999999323),z->(1,0.2)*sin(z),1.75,RandomLinearAnalogousHsl(1024,16,21),320,200)
	/// </example>
	public class JuliaTopographyFractal : FunctionMultiVariate
    {
		public JuliaTopographyFractal(ScriptNode z, ScriptNode c, ScriptNode dr, ScriptNode Palette,
			ScriptNode DimX, ScriptNode DimY, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { z, c, dr, Palette, DimX, DimY }, 
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar}, Start, Length, Expression)
        {
		}

		public JuliaTopographyFractal(ScriptNode z, ScriptNode c, ScriptNode dr, ScriptNode Palette,
			ScriptNode DimX, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, c, dr, Palette, DimX },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Vector, ArgumentType.Scalar}, Start, Length, Expression)
		{
		}

		public JuliaTopographyFractal(ScriptNode z, ScriptNode c, ScriptNode dr, ScriptNode Palette, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, c, dr, Palette },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Vector}, Start, Length, Expression)
		{
		}

		public JuliaTopographyFractal(ScriptNode z, ScriptNode c, ScriptNode dr, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, c, dr },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "z", "c", "dr", "Palette", "DimX", "DimY" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            string ColorExpression = null;
            SKColor[] Palette;
            double rc, ic;
            double r0, i0;
            double dr;
            int dimx, dimy;
            int i, c;
            ILambdaExpression f;
            ScriptNode fDef = null;
            object Obj;
            Complex z;

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
                throw new ScriptRuntimeException("Insufficient parameters in call to JuliaTopographyFractal().", this);

            Obj = Arguments[i++].AssociatedObjectValue;
            if ((f = Obj as ILambdaExpression) != null)
            {
                if (f.NrArguments != 1)
                    throw new ScriptRuntimeException("Lambda expression in calls to JuliaTopographyFractal() must be of one variable.", this);

                r0 = 0;
                i0 = 0;
                fDef = this.Arguments[i - 1];
            }
			else if (Obj is Complex)
			{
				z = (Complex)Obj;
				r0 = z.Real;
                i0 = z.Imaginary;
            }
            else
            {
                if (i >= c)
                    throw new ScriptRuntimeException("Insufficient parameters in call to JuliaTopographyFractal().", this);

                r0 = Expression.ToDouble(Obj);
                i0 = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
            }

            if (i >= c)
                throw new ScriptRuntimeException("Insufficient parameters in call to JuliaTopographyFractal().", this);

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
                throw new ScriptRuntimeException("Parameter mismatch in call to JuliaTopographyFractal(z,c|f,dr[,Palette][,dimx[,dimy]]).",
                    this);
            }

            if (dimx <= 0 || dimx > 5000 || dimy <= 0 || dimy > 5000)
                throw new ScriptRuntimeException("Image size must be within 1x1 to 5000x5000", this);

            if (!(f is null))
            {
                return CalcJulia(rc, ic, f, fDef, dr, Variables, Palette, dimx, dimy, this, 
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
            SKColor[] Palette = (SKColor[])Parameters[0];
            int DimX = (int)Parameters[1];
            int DimY = (int)Parameters[2];
            double r0 = (double)Parameters[3];
            double i0 = (double)Parameters[4];
            string ColorExpression = (string)Parameters[5];
            ScriptNode f = (ScriptNode)Parameters[6];

            StringBuilder sb = new StringBuilder();

            sb.Append("JuliaTopographyFractal((");
            sb.Append(Expression.ToString(r));
            sb.Append(",");
            sb.Append(Expression.ToString(i));
            sb.Append("),");
            
            if (!(f is null))
                sb.Append(f.SubExpression);
            else
            {
                sb.Append('(');
                sb.Append(Expression.ToString(r0));
                sb.Append(",");
                sb.Append(Expression.ToString(i0));
                sb.Append(')');
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

        public static FractalGraph CalcJulia(double rCenter, double iCenter, double R0, double I0, double rDelta,
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
            int index;

            N = Palette.Length;

            rDelta *= 0.5;
            r0 = rCenter - rDelta;
            r1 = rCenter + rDelta;

            aspect = ((double)Width) / Height;

            i0 = iCenter - rDelta / aspect;
            i1 = iCenter + rDelta / aspect;

            dr = (r1 - r0) / Width;
            di = (i1 - i0) / Height;

            int Size = Width * Height;
            int[] ColorIndex = new int[Size];

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

                    if (n >= N)
                        ColorIndex[index++]=N;
                    else
                        ColorIndex[index++]=n;
                }
            }

            int[] Boundary = FractalGraph.FindBoundaries(ColorIndex, Width, Height);

            return new FractalGraph(FractalGraph.ToBitmap(Boundary, Width, Height, Palette),
                r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
        }

        public static FractalGraph CalcJulia(double rCenter, double iCenter, ILambdaExpression f, 
            ScriptNode fDef, double rDelta, Variables Variables,
            SKColor[] Palette, int Width, int Height, ScriptNode Node,
            FractalZoomScript FractalZoomScript, object State)
        {
            double r0, i0, r1, i1;
            double dr, di;
            double r, i;
            double aspect;
            int x, y;
            int n, N;

            N = Palette.Length;

            rDelta *= 0.5;
            r0 = rCenter - rDelta;
            r1 = rCenter + rDelta;

            aspect = ((double)Width) / Height;

            i0 = iCenter - rDelta / aspect;
            i1 = iCenter + rDelta / aspect;

            dr = (r1 - r0) / Width;
            di = (i1 - i0) / Height;

            int Size = Width * Height;
            int[] ColorIndex = new int[Size];
            int c, j, x2;
            Complex z;
            IElement[] P = new IElement[1];
            IElement Obj;
            double Mod;

            for (y = 0, i = i0; y < Height; y++, i += di)
            {
                Complex[] Row = new Complex[Width];
                int[] Offset = new int[Width];

                c = Width;
                for (x = 0, x2 = y * Width, r = r0; x < Width; x++, r += dr, x2++)
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
                    P[0] = Expression.Encapsulate(Row);
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
                            ColorIndex[j++] = n;
                    }

                    if (x2 < x)
                    {
                        Array.Resize<Complex>(ref Row, x2);
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

            int[] Boundary = FractalGraph.FindBoundaries(ColorIndex, Width, Height);

            return new FractalGraph(FractalGraph.ToBitmap(Boundary, Width, Height, Palette),
                r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
        }

        public override string FunctionName
        {
            get { return "JuliaTopographyFractal"; }
        }
    }
}
