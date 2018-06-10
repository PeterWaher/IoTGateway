using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class DiscVariation : FlameVariationZeroParameters
    {
        public DiscVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y);
            double a = System.Math.Atan2(x, y);
            x = a * System.Math.Sin(r * System.Math.PI) / System.Math.PI;
            y = a * System.Math.Cos(r * System.Math.PI) / System.Math.PI;
        }

        public override string FunctionName
        {
            get { return "DiscVariation"; }
        }
    }
}
