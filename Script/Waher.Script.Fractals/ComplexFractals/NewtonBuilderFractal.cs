﻿using System;
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
    /// Calculates a Newton Fractal Image, but when clicked, adds a root to the underying
    /// polynomial, instead of zooming in.
    /// </summary>
    /// <example>
    /// NewtonBuilderFractal((0,0),3,)
    /// </example>
    public class NewtonBuilderFractal : FunctionMultiVariate
    {
		/// <summary>
		/// TODO
		/// </summary>
		public NewtonBuilderFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c, ScriptNode Palette,
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
		public NewtonBuilderFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c, ScriptNode Palette,
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
		public NewtonBuilderFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c, ScriptNode Palette,
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
		public NewtonBuilderFractal(ScriptNode z, ScriptNode dr, ScriptNode R, ScriptNode c,
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
		public NewtonBuilderFractal(ScriptNode z, ScriptNode dr, ScriptNode R, int Start, int Length,
			Expression Expression)
			: base(new ScriptNode[] { z, dr, R },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
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
		/// TODO
		/// </summary>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            string ColorExpression = null;
            SKColor[] Palette;
            double[] Coefficients = null;
            Complex[] CoefficientsZ = null;
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
                throw new ScriptRuntimeException("Insufficient parameters in call to NewtonBuilderFractal().", this);

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

			if (i < c && Arguments[i] is DoubleVector)
            {
                Coefficients = (double[])Arguments[i++].AssociatedObjectValue;

                int j, d = Coefficients.Length;

                CoefficientsZ = new Complex[d];
                for (j = 0; j < d; j++)
                    CoefficientsZ[j] = new Complex(Coefficients[j], 0);
            }
            else if (i < c && Arguments[i] is ComplexVector)
                CoefficientsZ = (Complex[])Arguments[i++].AssociatedObjectValue;
            /*else if (i < c && Parameters[i] is RealPolynomial)
                Coefficients = ((RealPolynomial)Arguments[i++].AssociatedObjectValue).Coefficients;
            else if (i < c && Parameters[i] is ComplexPolynomial)
                CoefficientsZ = ((ComplexPolynomial)Arguments[i++].AssociatedObjectValue).Coefficients;*/
            else if (i < c && Arguments[i] is IVector)
            {
				IVector Vector = (IVector)Arguments[i++];
				int j, d = Vector.Dimension;

				CoefficientsZ = new Complex[d];
				for (j = 0; j < d; j++)
					CoefficientsZ[j] = Expression.ToComplex(Vector.GetElement(j).AssociatedObjectValue);
			}
			else
                CoefficientsZ = Array.Empty<Complex>();

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
                throw new ScriptRuntimeException("Parameter mismatch in call to NewtonBuilderFractal(r,c,dr,Coefficients[,Palette][,dimx[,dimy]]).",
                    this);
            }

            if (dimx <= 0 || dimx > 5000 || dimy <= 0 || dimy > 5000)
                throw new ScriptRuntimeException("Image size must be within 1x1 to 5000x5000", this);

            return NewtonFractal.CalcNewton(Variables, rc, ic, dr, R, CoefficientsZ, Palette, dimx, dimy, this, this.FractalZoomScript,
                new object[] { Palette, dimx, dimy, R, CoefficientsZ, ColorExpression, rc, ic });
        }

        private string FractalZoomScript(double r, double i, double Size, object State)
        {
            object[] Parameters = (object[])State;
            int DimX = (int)Parameters[1];
            int DimY = (int)Parameters[2];
            Complex R = (Complex)Parameters[3];
            Complex[] CoefficientsZ = (Complex[])Parameters[4];
            string ColorExpression = (string)Parameters[5];
            double rc = (double)Parameters[6];
            double ic = (double)Parameters[7];
            Complex z0 = new Complex(r, i);
            int j;

            int c = CoefficientsZ.Length;
            while (c > 1 && CoefficientsZ[c - 1].Equals(0))
                c--;

            Complex[] C2;

            if (c < 1)
            {
                C2 = new Complex[2];
                C2[0] = -z0;
                C2[1] = Complex.One;
            }
            else
            {
                C2 = new Complex[c + 1];
                C2[0] = Complex.Zero;
                for (j = 0; j < c; j++)
                    C2[j + 1] = CoefficientsZ[j];

                for (j = 0; j < c; j++)
                    C2[j] -= z0 * CoefficientsZ[j];
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("NewtonBuilderFractal((");
            sb.Append(Expression.ToString(rc));
            sb.Append(',');
            sb.Append(Expression.ToString(ic));
            sb.Append("),");
            sb.Append(Expression.ToString(Size));
            sb.Append(',');
            sb.Append(Expression.ToString(R));
            sb.Append(',');
            sb.Append(Expression.ToString(C2));

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
		public override string FunctionName => nameof(NewtonBuilderFractal);
    }
}
