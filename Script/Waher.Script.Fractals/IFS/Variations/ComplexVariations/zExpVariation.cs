using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zExpVariation : FlameVariationZeroParameters
    {
        public zExpVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            // e^*(x+iy)
            double Mod = System.Math.Exp(x);
            x = Mod * System.Math.Cos(y);
            y = Mod * System.Math.Sin(y);
        }

        public override string FunctionName
        {
            get { return "zExpVariation"; }
        }
    }
}