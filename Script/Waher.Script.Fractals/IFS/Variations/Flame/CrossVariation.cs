using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class CrossVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public CrossVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double r = Math.Abs(x * x - y * y) + 1e-6;
            x /= r;
            y /= r;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(CrossVariation);
    }
}
