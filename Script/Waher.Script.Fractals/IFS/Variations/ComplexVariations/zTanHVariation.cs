using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class ZTanHVariation : FlameVariationZeroParameters
    {
        public ZTanHVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

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

        public override string FunctionName
        {
            get { return "zTanHVariation"; }
        }
    }
}