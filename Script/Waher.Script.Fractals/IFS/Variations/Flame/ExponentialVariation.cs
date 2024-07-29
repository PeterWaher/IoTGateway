using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class ExponentialVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public ExponentialVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double e = Math.Exp(x - 1);
            y *= Math.PI;
            x = e * Math.Cos(y);
            y = e * Math.Sin(y);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(ExponentialVariation);
    }
}
