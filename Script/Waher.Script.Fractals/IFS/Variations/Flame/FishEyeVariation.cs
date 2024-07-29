using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class FishEyeVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public FishEyeVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double r = 2 / (1 + Math.Sqrt(x * x + y * y));
            double x2 = r * y;
            y = r * x;
            x = x2;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(FishEyeVariation);
    }
}
