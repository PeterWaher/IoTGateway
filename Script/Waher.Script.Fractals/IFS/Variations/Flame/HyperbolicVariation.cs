using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class HyperbolicVariation : FlameVariationZeroParameters
    {
        public HyperbolicVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y) + 1e-6;
            double a = System.Math.Atan2(x, y);
            x = System.Math.Sin(a) / r;
            y = r * System.Math.Cos(a);
        }

        public override string FunctionName
        {
            get { return "HyperbolicVariation"; }
        }
    }
}
