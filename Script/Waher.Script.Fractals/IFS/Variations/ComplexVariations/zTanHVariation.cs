using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zTanHVariation : FlameVariationZeroParameters
    {
        public zTanHVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double Mod = System.Math.Exp(x);
            double zr = Mod * (System.Math.Cos(y));
            double zi = Mod * (System.Math.Sin(y));

            Mod = System.Math.Exp(-x);
            double zr2 = Mod * System.Math.Cos(-y);
            double zi2 = Mod * System.Math.Sin(-y);

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