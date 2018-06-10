using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zCosHVariation : FlameVariationZeroParameters
    {
        public zCosHVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double Mod = System.Math.Exp(x);
            double zr = Mod * (System.Math.Cos(y));
            double zi = Mod * (System.Math.Sin(y));

            Mod = System.Math.Exp(-x);
            zr += Mod * System.Math.Cos(-y);
            zi += Mod * System.Math.Sin(-y);

            x = zr * 0.5;
            y = zi * 0.5;
        }

        public override string FunctionName
        {
            get { return "zCosHVariation"; }
        }
    }
}