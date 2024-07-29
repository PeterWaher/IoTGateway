using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class HandkerchiefVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public HandkerchiefVariation(int Start, int Length, Expression Expression)
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
            x = r * Math.Sin(a + r);
            y = r * Math.Cos(a - r);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(HandkerchiefVariation);
    }
}
