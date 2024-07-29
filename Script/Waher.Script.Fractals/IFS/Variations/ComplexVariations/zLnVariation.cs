using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
	/// <summary>
	/// TODO
	/// </summary>
	public class ZLnVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public ZLnVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            // ln(x+iy)
            x = Math.Sqrt(x * x + y * y);
            y = Math.Atan2(y, x);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(ZLnVariation);
    }
}