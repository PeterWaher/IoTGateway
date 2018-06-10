using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zSinVariation : FlameVariationZeroParameters
    {
        public zSinVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double Mod = System.Math.Exp(-y);
            double zr = Mod * (System.Math.Cos(x));
            double zi = Mod * (System.Math.Sin(x));

            Mod = System.Math.Exp(y);
            zr -= Mod * System.Math.Cos(-x);
            zi -= Mod * System.Math.Sin(-x);

            x = zr * 0.5;
            y = zi * 0.5;
        }

        public override string FunctionName
        {
            get { return "zSinVariation"; }
        }
    }
}