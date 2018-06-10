using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class FishEyeVariation : FlameVariationZeroParameters
    {
        public FishEyeVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = 2 / (1 + System.Math.Sqrt(x * x + y * y));
            double x2 = r * y;
            y = r * x;
            x = x2;
        }

        public override string FunctionName
        {
            get { return "FishEyeVariation"; }
        }
    }
}
