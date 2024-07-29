using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class TangentVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public TangentVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double cy = Math.Cos(y) + 1e-6;
            x = Math.Sin(x) / cy;
            y = Math.Sin(y) / cy;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(TangentVariation);
    }
}
