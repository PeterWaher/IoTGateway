using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zTanVariation : FlameVariationZeroParameters
    {
        public zTanVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double Mod = System.Math.Exp(-y);
            double zr = Mod * System.Math.Cos(x);
            double zi = Mod * System.Math.Sin(x);

            Mod = System.Math.Exp(y);
            double zr2 = Mod * System.Math.Cos(-x);
            double zi2 = Mod * System.Math.Sin(-x);

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