using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class PopcornVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public PopcornVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double x2 = x + this.homogeneousTransform[2] * Math.Sin(Math.Tan(3 * y));
            y += this.homogeneousTransform[5] * Math.Sin(Math.Tan(3 * x));
            x = x2;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(PopcornVariation);
    }
}
