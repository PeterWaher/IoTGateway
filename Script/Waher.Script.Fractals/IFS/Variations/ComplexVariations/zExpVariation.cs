using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
	/// <summary>
	/// TODO
	/// </summary>
	public class ZExpVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public ZExpVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            // e^*(x+iy)
            double Mod = Math.Exp(x);
            x = Mod * Math.Cos(y);
            y = Mod * Math.Sin(y);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(ZExpVariation);
    }
}