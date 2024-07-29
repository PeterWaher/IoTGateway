using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class FanVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public FanVariation(int Start, int Length, Expression Expression)
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
            double t = this.homogeneousTransform[2];
            t = Math.PI * t * t + 1e-6;

            if (Math.IEEERemainder(a + this.homogeneousTransform[5], t) > t / 2)
                a -= t / 2;
            else
                a += t / 2;

            x = r * Math.Cos(a);
            y = r * Math.Sin(a);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(FanVariation);
    }
}
