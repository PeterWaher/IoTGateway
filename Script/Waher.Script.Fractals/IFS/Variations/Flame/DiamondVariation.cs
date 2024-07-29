using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class DiamondVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public DiamondVariation(int Start, int Length, Expression Expression)
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
            x = Math.Sin(a) * Math.Cos(r);
            y = Math.Cos(a) * Math.Sin(r);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(DiamondVariation);
    }
}
