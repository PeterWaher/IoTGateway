using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class HorseShoeVariation : FlameVariationZeroParameters
    {
        public HorseShoeVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y) + 1e-6;
            double x2 = (x - y) * (x + y) / r;
            y = 2 * x * y / r;
            x = x2;
        }

        public override string FunctionName
        {
            get { return "HorseShoeVariation"; }
        }
    }
}
