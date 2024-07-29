using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class RingsVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public RingsVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            double a = Math.Atan2(x, y);
            double c2 = this.homogeneousTransform[2];
            c2 = c2 * c2 + 1e-6;
            r = Math.IEEERemainder(r + c2, 2 * c2) - c2 + r * (1 - c2);
            x = r * Math.Cos(a);
            y = r * Math.Sin(a);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(RingsVariation);
    }
}
