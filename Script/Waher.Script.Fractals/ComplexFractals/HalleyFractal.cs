using System;
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
    /// Calculates a Halley Fractal Image
    /// </summary>
    /// <example>
    /// HalleyFractal((0,0),3,,[-1,0,0,1])
    /// HalleyFractal((0,0),3,,[-1,0,0,0,1])
    /// HalleyFractal((0,0),3,,[-1,0,0,0,0,1])
    /// HalleyFractal((0,0),3,,[-1,0,0,0,0,0,1])
    /// HalleyFractal((0,0),3,,[-1,0,0,0,0,0,0,1])
    /// HalleyFractal((0,0),5,,[2,-2,0,1])
    /// HalleyFractal((0,0),5,,[-16,0,0,0,15,0,0,0,1])
    /// HalleyFractal((0,0),10,,[-16,0,0,0,15,0,0,0,1])
    /// HalleyFractal((0,0),3,,print(Uniform(0,5,8)),,640,480)
    /// HalleyFractal((0,0),3,,print(Uniform(0,5,8)),RandomLinearRGB(1024,4,1325528060), 640,480)
    /// HalleyFractal((-0.86953125,0.00190625),0.75,1,[1.98185443271969, -4.98404552693667, 0.610523957577778, 1.03580189218549, 2.46752347213566, -0.452840749850888, -4.44446179524272, -4.55514865441022, 0.276169085538093, -2.77822038986637, -0.932593483446443, 0.590724170017394, 4.45106311210015, 1.22275979082229, -1.58897754577407, -0.284485842699411, -3.90592132178411, 0.364667687269239, 4.22900414989749, -0.546055233825955],RandomLinearRGB(1024,4,1325528060),640,480)
    ///
    /// Complex polynomials:
    /// HalleyFractal((0,0),10,,[-16,0,0,0,15*i,0,0,0,1])
    /// HalleyFractal((0,0),3,,Uniform(-5,5,8)+i*Uniform(-5,5,8))
    /// 
    /// Generalized Halley Fractals:
    /// HalleyFractal((0,0),3,0.5,[-1,0,0,1],,640,480)
    /// HalleyFractal((0,0),3,1.5,[-1,0,0,1],,640,480)
    /// HalleyFractal((0,0),3,0.5,Uniform(0,5,8),,640,480)
    /// HalleyFractal((0,0),3,1.5,Uniform(0,5,8),,640,480)
    /// 
    /// Using Lambda expressions:
    /// HalleyFractal((pi/2,0),pi/4,,x->sin(x),RandomLinearRGB(128,16,1177860657),640,480)
    /// </example>
    public class HalleyFractal : FunctionMultiVariate
    {
		public HalleyFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode Coefficients,
			ScriptNode Palette, ScriptNode DimX, ScriptNode DimY, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { z, dr, R, Coefficients, Palette, DimX, DimY }, 
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal, ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar}, Start, Length, Expression)
        {
		}

		public HalleyFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode Coefficients,
			ScriptNode Palette, ScriptNode DimX, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R, Coefficients, Palette, DimX },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal, ArgumentType.Vector, ArgumentType.Scalar}, Start, Length, Expression)
		{
		}

		public HalleyFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode Coefficients,
			ScriptNode Palette, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R, Coefficients, Palette },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal, ArgumentType.Vector}, Start, Length, Expression)
		{
		}

		public HalleyFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode Coefficients,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R, Coefficients },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Normal}, Start, Length, Expression)
		{
		}

		public HalleyFractal(ScriptNode z, ScriptNode dr, ScriptNode R, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr, R },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		public HalleyFractal(ScriptNode z, ScriptNode dr, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { z, dr },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "z", "dr", "R", "Coefficients", "Palette", "DimX", "DimY" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
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
                throw new ScriptRuntimeException("Insufficient parameters in call to HalleyFractal().", this);

            dr = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);

            if (i < c && (Arguments[i] is DoubleNumber || Arguments[i] is ComplexNumber))
                R = Expression.ToComplex(Arguments[i++].AssociatedObjectValue);
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
						throw new ScriptRuntimeException("Lambda expression in calls to HalleyFractal() must be of one variable.", this);

					fDef = this.Arguments[i++];
				}
				else
				{
					throw new ScriptRuntimeException("Parameter " + (i + 1).ToString() +
						" in call to HalleyFractal has to be a vector of numbers, containing coefficients " +
						"of the polynomial to use. Now it was of type " + Arguments[i].GetType().FullName,
						this);
				}
			}
			else
				throw new ScriptRuntimeException("Missing coefficients or lambda expression.", this);

			if (i < c && this.Arguments[i] != null && Arguments[i] is ObjectVector)
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
                throw new ScriptRuntimeException("Parameter mismatch in call to HalleyFractal(z,dr[,R][,Coefficients][,Palette][,dimx[,dimy]]).",
                    this);
            }

            if (dimx <= 0 || dimx > 5000 || dimy <= 0 || dimy > 5000)
                throw new ScriptRuntimeException("Image size must be within 1x1 to 5000x5000", this);

            if (f != null)
            {
                return CalcHalley(rc, ic, dr, R, f, fDef, Variables, Palette, dimx, dimy,
                   this, this.FractalZoomScript,
                   new object[] { Palette, dimx, dimy, R, fDef, ColorExpression });
            }
            else if (CoefficientsZ != null)
            {
                return CalcHalley(rc, ic, dr, R, CoefficientsZ, Palette, dimx, dimy,
                   this, this.FractalZoomScript,
                   new object[] { Palette, dimx, dimy, R, CoefficientsZ, ColorExpression });
            }
            else
            {
                return CalcHalley(rc, ic, dr, R, Coefficients, Palette, dimx, dimy,
                   this, this.FractalZoomScript,
                   new object[] { Palette, dimx, dimy, R, Coefficients, ColorExpression });
            }
        }

        private string FractalZoomScript(double r, double i, double Size, object State)
        {
            object[] Parameters = (object[])State;
            SKColor[] Palette = (SKColor[])Parameters[0];
            int DimX = (int)Parameters[1];
            int DimY = (int)Parameters[2];
            Complex R = (Complex)Parameters[3];
            double[] Coefficients = Parameters[4] as double[];
            Complex[] CoefficientsZ = Parameters[4] as Complex[];
			string ColorExpression = (string)Parameters[5];

			StringBuilder sb = new StringBuilder();

            sb.Append("HalleyFractal(");
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

        public static FractalGraph CalcHalley(double rCenter, double iCenter, double rDelta, Complex R,
            double[] Coefficients, SKColor[] Palette, int Width, int Height, ScriptNode Node, 
            FractalZoomScript FractalZoomScript, object State)
        {
            byte[] reds;
            byte[] greens;
            byte[] blues;
            double RRe = R.Real;
            double RIm = R.Imaginary;
            double r0, i0, r1, i1;
            double dr, di;
            double r, i;
            double zr, zi, zr2, zi2, zr3, zi3, zr4, zi4, zr5, zi5, zr6, zi6;
            double aspect;
            double Temp;
            int x, y;
            int n, N;
            int index;
            int Degree = Coefficients.Length - 1;
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

            if (Degree < 3)
            {
                Array.Resize<double>(ref Coefficients, 4);
                while (Degree < 3)
                    Coefficients[++Degree] = 0;
            }

            double[] Prim = new double[Degree];
            for (x = 1; x <= Degree; x++)
                Prim[x - 1] = x * Coefficients[x];

            double[] Bis = new double[Degree - 1];
            for (x = 1; x < Degree; x++)
                Bis[x - 1] = x * Prim[x];

            Array.Reverse(Bis);
            Array.Reverse(Prim);
            Coefficients = (double[])Coefficients.Clone();
            Array.Reverse(Coefficients);

            int size = Width * Height * 4;
            double Conv = 1e-10;
            double Div = 1e10;
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

                        // f":
                        zr4 = zi4 = 0;
                        foreach (double C in Bis)
                        {
                            Temp = zr4 * zr - zi4 * zi + C;
                            zi4 = zr4 * zi + zi4 * zr;
                            zr4 = Temp;
                        }

                        // 2*f*f'

                        zr5 = 2 * (zr2 * zr3 - zi2 * zi3);
                        zi5 = 2 * (zr2 * zi3 + zi2 * zr3);

                        // 2f'^2-f*f"

                        zr6 = 2 * (zr3 * zr3 - zi3 * zi3) - (zr2 * zr4 - zi2 * zi4);
                        zi6 = 4 * zr3 * zi3 - (zr2 * zi4 + zr4 * zi2);

                        // R*2*f*f'/2f'^2-f*f"

                        Temp = 1.0 / (zr6 * zr6 + zi6 * zi6);
                        zr4 = (zr5 * zr6 + zi5 * zi6) * Temp;
                        zi4 = (zi5 * zr6 - zr5 * zi6) * Temp;

                        // R*2*f*f'/(2f'^2-f*f")
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

			using (SKData Data = SKData.Create(new MemoryStream(rgb)))
			{
				SKImage Bitmap = SKImage.FromPixelData(new SKImageInfo(Width, Height, SKColorType.Bgra8888), Data, Width * 4);
				return new FractalGraph(Bitmap, r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
			}
		}

        public static FractalGraph CalcHalley(double rCenter, double iCenter, double rDelta, Complex R,
            Complex[] Coefficients, SKColor[] Palette, int Width, int Height, ScriptNode Node,
            FractalZoomScript FractalZoomScript, object State)
        {
            byte[] reds;
            byte[] greens;
            byte[] blues;
            double RRe = R.Real;
            double RIm = R.Imaginary;
            double r0, i0, r1, i1;
            double dr, di;
            double r, i;
            double zr, zi, zr2, zi2, zr3, zi3, zr4, zi4, zr5, zi5, zr6, zi6;
            double aspect;
            double Temp;
            int x, y;
            int n, N;
            int index;
            int Degree = Coefficients.Length - 1;
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

            if (Degree < 3)
            {
                Array.Resize<Complex>(ref Coefficients, 4);
                while (Degree < 3)
                    Coefficients[++Degree] = Complex.Zero;
            }

            Complex[] Prim = new Complex[Degree];
            for (x = 1; x <= Degree; x++)
                Prim[x - 1] = x * Coefficients[x];

            Complex[] Bis = new Complex[Degree - 1];
            for (x = 1; x < Degree; x++)
                Bis[x - 1] = x * Prim[x];

            Array.Reverse(Bis);
            Array.Reverse(Prim);
            Coefficients = (Complex[])Coefficients.Clone();
            Array.Reverse(Coefficients);

            int j, c = Prim.Length;
            int c2 = c - 1;
            double[] ReC = new double[c + 1];
            double[] ImC = new double[c + 1];
            double[] RePrim = new double[c];
            double[] ImPrim = new double[c];
            double[] ReBis = new double[c2];
            double[] ImBis = new double[c2];
            Complex z;

            for (j = 0; j < c; j++)
            {
                z = Coefficients[j];
                ReC[j] = z.Real;
                ImC[j] = z.Imaginary;

                z = Prim[j];
                RePrim[j] = z.Real;
                ImPrim[j] = z.Imaginary;

                if (j < c2)
                {
                    z = Bis[j];
                    ReBis[j] = z.Real;
                    ImBis[j] = z.Imaginary;
                }
            }

            z = Coefficients[j];
            ReC[j] = z.Real;
            ImC[j] = z.Imaginary;

            int size = Width * Height * 4;
            double Conv = 1e-10;
            double Div = 1e10;
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

                        // f":
                        zr4 = zi4 = 0;
                        for (j = 0; j < c2; j++)
                        {
                            Temp = zr4 * zr - zi4 * zi + ReBis[j];
                            zi4 = zr4 * zi + zi4 * zr + ImBis[j];
                            zr4 = Temp;
                        }

                        // 2*f*f'

                        zr5 = 2 * (zr2 * zr3 - zi2 * zi3);
                        zi5 = 2 * (zr2 * zi3 + zi2 * zr3);

                        // 2f'^2-f*f"

                        zr6 = 2 * (zr3 * zr3 - zi3 * zi3) - (zr2 * zr4 - zi2 * zi4);
                        zi6 = 4 * zr3 * zi3 - (zr2 * zi4 + zr4 * zi2);

                        // R*2*f*f'/2f'^2-f*f"

                        Temp = 1.0 / (zr6 * zr6 + zi6 * zi6);
                        zr4 = (zr5 * zr6 + zi5 * zi6) * Temp;
                        zi4 = (zi5 * zr6 - zr5 * zi6) * Temp;

                        // R*2*f*f'/(2f'^2-f*f")
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

			using (SKData Data = SKData.Create(new MemoryStream(rgb)))
			{
				SKImage Bitmap = SKImage.FromPixelData(new SKImageInfo(Width, Height, SKColorType.Bgra8888), Data, Width * 4);
				return new FractalGraph(Bitmap, r0, i0, r1, i1, rDelta * 2, true, Node, FractalZoomScript, State);
			}
		}

        public static FractalGraph CalcHalley(double rCenter, double iCenter, double rDelta, Complex R,
            ILambdaExpression f, ScriptNode fDef, Variables Variables, SKColor[] Palette, int Width, int Height, 
            ScriptNode Node, FractalZoomScript FractalZoomScript, object State)
        {
            byte[] reds;
            byte[] greens;
            byte[] blues;
            double RRe = R.Real;
            double RIm = R.Imaginary;
            double r0, i0, r1, i1;
            double dr, di;
            double r, i;
            double aspect;
            int x, y;
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

            Variables v = new Variables();
            Variables.CopyTo(v);

            string ParameterName;
			if (!(f is IDifferentiable Differentiable) ||
				!(Differentiable.Differentiate(ParameterName = Differentiable.DefaultVariableName, v) is ILambdaExpression fPrim))
			{
				throw new ScriptRuntimeException("Lambda expression not differentiable.", Node);
			}

			if (!(fPrim is IDifferentiable Differentiable2) ||
				!(Differentiable2.Differentiate(ParameterName, v) is ILambdaExpression fBis))
			{
				throw new ScriptRuntimeException("Lambda expression not twice differentiable.", Node);
			}

			int size = Width * Height * 4;
            double Conv = 1e-10;
            double Div = 1e10;
            byte[] rgb = new byte[size];

            Complex[] Row;
            Complex[] Row2;
            Complex[] Row3;
            Complex[] Row4;
            int[] Offset;
            IElement[] P = new IElement[1];
            int j, c, x2;
            IElement Obj, Obj2, Obj3;
            double Mod;
            Complex z, z2, z3;
            Complex R2 = R * 2;

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
                    Obj3 = fBis.Evaluate(P, v);
                    Row2 = Obj.AssociatedObjectValue as Complex[];
                    Row3 = Obj2.AssociatedObjectValue as Complex[];
                    Row4 = Obj3.AssociatedObjectValue as Complex[];

                    if (Row2 is null || Row3 is null || Row4 is null)
                    {
                        throw new ScriptRuntimeException("Lambda expression (and its first and second derivative) must be able to accept complex vectors, " +
                            "and return complex vectors of equal length. Type returned: " +
                            Obj.GetType().FullName + ", " + Obj2.GetType().FullName + " and " + Obj3.GetType().FullName, 
                            Node);
                    }
                    else if (Row2.Length != c || Row3.Length != c)
                    {
                        throw new ScriptRuntimeException("Lambda expression (and its first and second derivative) must be able to accept complex vectors, " +
                            "and return complex vectors of equal length. Length returned: " +
                            Row2.Length.ToString() + ", " + Row3.Length.ToString() + " and " + Row4.Length.ToString() +
                            ". Expected: " + c.ToString(), Node);
                    }

                    for (x = x2 = 0; x < c; x++)
                    {
                        j = Offset[x];
                        z = Row2[x];
                        z2 = Row3[x];
                        z3 = Row4[x];

                        z = R2 * z * z2 / (2 * z2 * z2 - z * z3);
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
                                rgb[j++] = blues[n];
                                rgb[j++] = greens[n];
                                rgb[j++] = reds[n];
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
            get { return "HalleyFractal"; }
        }
    }
}
