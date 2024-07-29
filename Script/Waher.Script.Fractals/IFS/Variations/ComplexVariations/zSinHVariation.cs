using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
	/// <summary>
	/// TODO
	/// </summary>
	public class ZSinHVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public ZSinHVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double Mod = Math.Exp(x);
            double zr = Mod * (Math.Cos(y));
            double zi = Mod * (Math.Sin(y));

            Mod = Math.Exp(-x);
            zr -= Mod * Math.Cos(-y);
            zi -= Mod * Math.Sin(-y);

            x = zr * 0.5;
            y = zi * 0.5;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(ZSinHVariation);
    }
}