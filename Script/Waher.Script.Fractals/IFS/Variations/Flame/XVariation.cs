using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class XVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public XVariation(int Start, int Length, Expression Expression)
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
            double p0 = Math.Sin(a + r);
            double p1 = Math.Cos(a - r);
            p0 = p0 * p0 * p0;
            p1 = p1 * p1 * p1;
            x = r * (p0 + p1);
            y = r * (p0 - p1);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(XVariation);
    }
}
