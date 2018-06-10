using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class HeartVariation : FlameVariationZeroParameters
    {
        public HeartVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y);
            double a = System.Math.Atan2(x, y);
            x = r * System.Math.Sin(r * a);
            y = -r * System.Math.Cos(r * a);
        }

        public override string FunctionName
        {
            get { return "HeartVariation"; }
        }
    }
}
