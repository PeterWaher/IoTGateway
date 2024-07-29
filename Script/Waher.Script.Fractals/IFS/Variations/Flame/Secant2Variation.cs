using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class Secant2Variation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public Secant2Variation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y) * this.variationWeight;
            double c = Math.Cos(r);
            if (c < 0)
                y = 1 / (c + 1);
            else
                y = 1 / (c - 1);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(Secant2Variation);
    }
}
