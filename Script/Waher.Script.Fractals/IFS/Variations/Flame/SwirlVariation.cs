using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class SwirlVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public SwirlVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double r = x * x + y * y;
            double s = Math.Sin(r);
            double c = Math.Cos(r);
            double x2 = x * s - y * c;
            y = x * c + y * s;
            x = x2;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(SwirlVariation);
    }
}
