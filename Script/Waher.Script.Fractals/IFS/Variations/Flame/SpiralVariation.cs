using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class SpiralVariation : FlameVariationZeroParameters
    {
        public SpiralVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y) + 1e-6;
            double a = System.Math.Atan2(x, y);
            x = (System.Math.Cos(a) + System.Math.Sin(r)) / r;
            y = (System.Math.Sin(a) - System.Math.Cos(r)) / r;
        }

        public override string FunctionName
        {
            get { return "SpiralVariation"; }
        }
    }
}
