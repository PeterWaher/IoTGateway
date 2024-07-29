using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class SinusoidalVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public SinusoidalVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            x = Math.Sin(x);
            y = Math.Sin(y);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(SinusoidalVariation);
    }
}
