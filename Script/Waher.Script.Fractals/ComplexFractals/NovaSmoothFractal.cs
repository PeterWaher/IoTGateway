using System;
using System.Numerics;
using System.Text;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Fractals.ComplexFractals
{
    /// <summary>
    /// Calculates a Nova Smooth Fractal Image
    /// </summary>
    /// <example>
    /// NovaSmoothFractal(0,0,3,1,2)
    /// NovaSmoothFractal(0,0,3,1,3)
    /// NovaSmoothFractal(0,0,3,1,5)
    /// NovaSmoothFractal(0,0,3,1,6,,640,480)
    /// NovaSmoothFractal(0,0,3,1,7,,640,480)
    /// NovaSmoothFractal(0,0,3,1,8,,640,480)
    /// 
    /// Generalized Nova Fractals:
    /// NovaSmoothFractal(0,0,3,0.5,2,,640,480)
    /// NovaSmoothFractal(0,0,3,1.5,3,,640,480)
    /// NovaSmoothFractal(0,0,3,1.5,4,,640,480)
    /// 
    /// NovaSmoothFractal(-0.0238669395446776,-0.322384971618653,4.57763671875E-05,1,4+3*i,randomlinearrgb(1024,12,976253670),1024,768)
    /// </example>
    public class NovaSmoothFractal : FunctionMultiVariate
    {
		public NovaSmoothFractal(ScriptNode r, ScriptNode i, ScriptNode dr, ScriptNode R, ScriptNode p,
			ScriptNode Palette, ScriptNode DimX, ScriptNode DimY, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { r, i, dr, R, p, Palette, DimX, DimY }, 
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Vector, ArgumentType.Scalar,
				  ArgumentType.Scalar}, Start, Length, Expression)
        {
		}

		public NovaSmoothFractal(ScriptNode r, ScriptNode i, ScriptNode dr, ScriptNode R, ScriptNode p,
			ScriptNode Palette, ScriptNode DimX, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { r, i, dr, R, p, Palette, DimX },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Vector, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		public NovaSmoothFractal(ScriptNode r, ScriptNode i, ScriptNode dr, ScriptNode R, ScriptNode p,
			ScriptNode Palette, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { r, i, dr, R, p, Palette },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		public NovaSmoothFractal(ScriptNode r, ScriptNode i, ScriptNode dr, ScriptNode R, ScriptNode p,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { r, i, dr, R, p },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "r", "i", "dr", "R", "p", "Palette", "DimX", "DimY" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            string ColorExpression = null;
            SKColor[] Palette;
			object Obj;
            double rc, ic;
            double dr;
            double Rr, Ri, pr, pi;
            int dimx, dimy;
            int i, c;

            rc = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            ic = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
            dr = Expression.ToDouble(Arguments[2].AssociatedObjectValue);

			if ((Obj = Arguments[3].AssociatedObjectValue) is Complex)
			{
				Complex z = (Complex)Obj;
				Rr = z.Real;
                Ri = z.Imaginary;
            }
            else
            {
                Rr = Expression.ToDouble(Arguments[3].AssociatedObjectValue);
                Ri = 0;
            }

			if ((Obj = Arguments[4].AssociatedObjectValue) is Complex)
			{
				Complex z = (Complex)Obj;
				pr = z.Real;
                pi = z.Imaginary;
            }
            else
            {
                pr = Expression.ToDouble(Arguments[4].AssociatedObjectValue);
                pi = 0;
            }

            c = Arguments.Length;
            i = 5;

			if (i < c && this.Arguments[i] != null && Arguments[i] is ObjectVector)
			{
				ColorExpression = this.Arguments[i].SubExpression;
                Palette = FractalGraph.ToPalette((ObjectVector)Arguments[i++]);
            }
            else
            {
                Palette = ColorModels.RandomLinearAnalogousHSL.CreatePalette(128, 4, out int Seed, this, Variables);
                ColorExpression = "RandomLinearAnalogousHSL(128,4," + Seed.ToString() + ")";

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
                throw new ScriptRuntimeException("Parameter mismatch in call to NovaSmoothFractal(r,c,dr,Coefficients[,Palette][,dimx[,dimy]]).",
                    this);
            }

            if (dimx <= 0 || dimx > 5000 || dimy <= 0 || dimy > 5000)
                throw new ScriptRuntimeException("Image size must be within 1x1 to 5000x5000", this);

            return CalcNova(rc, ic, dr, Rr, Ri, pr, pi, Palette, dimx, dimy, this, Variables, this.FractalZoomScript, 
                new object[] { Palette, dimx, dimy, Rr, Ri, pr, pi, ColorExpression });
        }

        private string FractalZoomScript(double r, double i, double Size, object State)
        {
            object[] Parameters = (object[])State;
            SKColor[] Palette = (SKColor[])Parameters[0];
            int DimX = (int)Parameters[1];
            int DimY = (int)Parameters[2];
            double Rr = (double)Parameters[3];
            double Ri = (double)Parameters[4];
            double pr = (double)Parameters[5];
            double pi = (double)Parameters[6];
            string ColorExpression = (string)Parameters[7];

            StringBuilder sb = new StringBuilder();

            sb.Append("NovaSmoothFractal(");
            sb.Append(Expression.ToString(r));
            sb.Append(",");
            sb.Append(Expression.ToString(i));
            sb.Append(",");
            sb.Append(Expression.ToString(Size / 4));
            sb.Append(",");
            sb.Append(Expression.ToString(Rr));

            if (Ri != 0)
            {
                sb.Append("+");
                sb.Append(Expression.ToString(Ri));
                sb.Append("*i");
            }

            sb.Append(",");
            sb.Append(Expression.ToString(pr));

            if (pi != 0)
            {
                sb.Append("+");
                sb.Append(Expression.ToString(pi));
                sb.Append("*i");
            }
            
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

        public static FractalGraph CalcNova(double rCenter, double iCenter, double rDelta, double Rr, double Ri,
            double pr, double pi, SKColor[] Palette, int Width, int Height, ScriptNode Node, Variables Variables,
            FractalZoomScript FractalZoomScript, object State)
        {
            double r0, i0, r1, i1;
            double dr, di;
            double r, i;
            double zr, zi, zr2, zi2, zr3, zi3, zr4, zi4;
            double aspect;
            double Temp;
            int x, y;
            int n, N;
            int index;
            double lnz;
            double argz;
            double amp;
            double phi;

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
            double[] ColorIndex = new double[Size];
            double Conv = 1e-10;
            double Div = 1e10;

            for (y = 0, i = i0, index = 0; y < Height; y++, i += di)
            {
                for (x = 0, r = r0; x < Width; x++, r += dr)
                {
                    zr = r;
                    zi = i;

                    n = 0;
                    do
                    {
                        // f: z->z^p-1 = exp(p*ln(z))-1
                        // exp(a+ib)=exp(a)*(cos(b)+i*sin(b))
                        // ln(z)=ln|z|+i*arg(z)
                        // exp(p*ln(z))-1 =
                        // = exp((pr+i*pi)*(ln|z|+i*arg(z)))-1 =
                        // = exp(pr*ln|z|-pi*arg(z)+i*(pi*ln|z|+pr*arg(z)))-1 =
                        // = exp(pr*ln|z|-pi*arg(z))*(cos(pi*ln|z|+pr*arg(z))+i*sin(pi*ln|z|+pr*arg(z)))-1

                        lnz = System.Math.Log(Math.Sqrt(zr * zr + zi * zi));
                        argz = System.Math.Atan2(zi, zr);
                        amp = System.Math.Exp(pr * lnz - pi * argz);
                        phi = pi * lnz + pr * argz;

                        zr2 = amp * System.Math.Cos(phi) - 1;
                        zi2 = amp * System.Math.Sin(phi);

                        // f': z->p*z^(p-1) = p*exp((p-1)*ln(z)) =
                        // = (pr+i*pi)*exp((pr-1+i*pi)*(ln|z|+i*arg(z))) =
                        // = (pr+i*pi)*exp((pr-1)*ln|z|-pi*arg(z)+i*(pi*ln|z|+(pr-1)*arg(z))) =
                        // = (pr+i*pi)*exp((pr-1)*ln|z|-pi*arg(z))(sin(pi*ln|z|+(pr-1)*arg(z))+i*cos(pi*ln|z|+(pr-1)*arg(z))) =

                        amp = System.Math.Exp((pr - 1) * lnz - pi * argz);
                        phi = pi * lnz + (pr - 1) * argz;

                        zr3 = amp * System.Math.Cos(phi);
                        zi3 = amp * System.Math.Sin(phi);

                        Temp = pr * zr3 - pi * zi3;
                        zi3 = pr * zi3 + pi * zr3;
                        zr3 = Temp;

                        // f/f':

                        Temp = 1.0 / (zr3 * zr3 + zi3 * zi3);
                        zr4 = (zr2 * zr3 + zi2 * zi3) * Temp;
                        zi4 = (zi2 * zr3 - zr2 * zi3) * Temp;

                        Temp = Rr * zr4 - Ri * zi4;
                        zi4 = Ri * zr4 + Rr * zi4;
                        zr4 = Temp;

                        zr -= zr4;
                        zi -= zi4;

                        Temp = Math.Sqrt(zr4 * zr4 + zi4 * zi4);
                    }
                    while ((Temp > Conv) && (Temp < Div) && (n++ < N));

                    if (Temp < Conv && n < N)
                        ColorIndex[index++] = n;
                    else
                        ColorIndex[index++] = N;
                }
            }

            Node.Expression.Preview(new GraphBitmap(FractalGraph.ToBitmap(ColorIndex, Width, Height, Palette)));

            double[] Boundary = FractalGraph.FindBoundaries(ColorIndex, Width, Height);
            FractalGraph.Smooth(ColorIndex, Boundary, Width, Height, N, Palette, Node, Variables);

            return new FractalGraph(FractalGraph.ToBitmap(ColorIndex, Width, Height, Palette),
                r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
        }

        public override string FunctionName
        {
            get { return "NovaSmoothFractal"; }
        }
    }
}
