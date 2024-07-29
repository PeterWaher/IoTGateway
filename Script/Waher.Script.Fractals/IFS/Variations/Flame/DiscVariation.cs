using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class DiscVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public DiscVariation(int Start, int Length, Expression Expression)
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
            x = a * Math.Sin(r * Math.PI) / Math.PI;
            y = a * Math.Cos(r * Math.PI) / Math.PI;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(DiscVariation);
    }
}
