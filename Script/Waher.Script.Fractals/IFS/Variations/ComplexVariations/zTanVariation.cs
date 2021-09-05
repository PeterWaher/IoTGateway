using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class ZTanVariation : FlameVariationZeroParameters
    {
        public ZTanVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double Mod = Math.Exp(-y);
            double zr = Mod * Math.Cos(x);
            double zi = Mod * Math.Sin(x);

            Mod = Math.Exp(y);
            double zr2 = Mod * Math.Cos(-x);
            double zi2 = Mod * Math.Sin(-x);

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
            get { return "zTanVariation"; }
        }
    }
}