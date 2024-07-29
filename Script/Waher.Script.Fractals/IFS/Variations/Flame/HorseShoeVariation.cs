using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class HorseShoeVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public HorseShoeVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y) + 1e-6;
            double x2 = (x - y) * (x + y) / r;
            y = 2 * x * y / r;
            x = x2;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(HorseShoeVariation);
    }
}
