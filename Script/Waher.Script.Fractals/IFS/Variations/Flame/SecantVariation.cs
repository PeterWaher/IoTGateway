using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class SecantVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public SecantVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            y = 1 / (this.variationWeight * Math.Cos(this.variationWeight * r));
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(SecantVariation);
    }
}
