using System;
using System.Collections.Generic;
using System.Numerics;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
	public class zACosVariation : FlameVariationZeroParameters
	{
		public zACosVariation(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		public override void Operate(ref double x, ref double y)
		{
			Complex z = new Complex(x, y);
			z = Complex.Log(z * Complex.ImaginaryOne + Complex.Sqrt(1 - z * z));

			x = 0.5 * System.Math.PI - z.Imaginary;
			y = z.Real;
		}

		public override string FunctionName
		{
			get { return "zACosVariation"; }
		}
	}
}