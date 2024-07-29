using System;
using System.Numerics;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
	/// <summary>
	/// TODO
	/// </summary>
	public class ZACosVariation : FlameVariationZeroParameters
	{
		/// <summary>
		/// TODO
		/// </summary>
		public ZACosVariation(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
		{
			Complex z = new Complex(x, y);
			z = Complex.Log(z * Complex.ImaginaryOne + Complex.Sqrt(1 - z * z));

			x = 0.5 * Math.PI - z.Imaginary;
			y = z.Real;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(ZACosVariation);
	}
}