using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class HyperbolicVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public HyperbolicVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y) + 1e-6;
            double a = Math.Atan2(x, y);
            x = Math.Sin(a) / r;
            y = r * Math.Cos(a);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(HyperbolicVariation);
    }
}
