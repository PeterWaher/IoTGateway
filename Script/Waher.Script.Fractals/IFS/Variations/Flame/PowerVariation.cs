using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class PowerVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public PowerVariation(int Start, int Length, Expression Expression)
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
            double s = Math.Sin(a);
            r = Math.Pow(r, s);
            x = r * Math.Cos(a);
            y = r * s;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(PowerVariation);
    }
}
