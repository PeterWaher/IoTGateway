using System.Numerics;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{   
	/// <summary>
	/// TODO
	/// </summary>
	public class ZASinVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public ZASinVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            Complex z = new Complex(x, y);
            z = -Complex.Log(z * Complex.ImaginaryOne + Complex.Sqrt(1 - z * z)) * Complex.ImaginaryOne;

            x = z.Real;
            y = z.Imaginary;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(ZASinVariation);
    }
}