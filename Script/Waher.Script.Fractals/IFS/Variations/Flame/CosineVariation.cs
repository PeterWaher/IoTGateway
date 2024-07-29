using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class CosineVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public CosineVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            x *= Math.PI;
            double x2 = Math.Cos(x) * Math.Cosh(y);
            y = -Math.Sin(x) * Math.Sinh(y);
            x = x2;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(CosineVariation);
    }
}
