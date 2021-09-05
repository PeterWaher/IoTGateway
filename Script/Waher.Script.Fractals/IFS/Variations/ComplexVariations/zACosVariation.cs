using System;
using System.Numerics;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
	public class ZACosVariation : FlameVariationZeroParameters
	{
		public ZACosVariation(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		public override void Operate(ref double x, ref double y)
		{
			Complex z = new Complex(x, y);
			z = Complex.Log(z * Complex.ImaginaryOne + Complex.Sqrt(1 - z * z));

			x = 0.5 * Math.PI - z.Imaginary;
			y = z.Real;
		}

		public override string FunctionName
		{
			get { return "zACosVariation"; }
		}
	}
}