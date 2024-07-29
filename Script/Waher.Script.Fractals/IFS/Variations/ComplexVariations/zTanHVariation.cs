using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
	/// <summary>
	/// TODO
	/// </summary>
	public class ZTanHVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public ZTanHVariation(int Start, int Length, Expression Expression)
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
            double zr2 = Mod * Math.Cos(-y);
            double zi2 = Mod * Math.Sin(-y);

            double sinr = zr - zr2;
            double sini = zi - zi2;

            double cosr = zr + zr2;
            double cosi = zi + zi2;

            double d = 1.0 / (cosr * cosr + cosi * cosi);

            x = (sinr * cosr + sini * cosi) * d;
            y = (sini * cosr - sinr * cosi) * d;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(ZTanHVariation);
    }
}