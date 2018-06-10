using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class RingsVariation : FlameVariationZeroParameters
    {
        public RingsVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y);
            double a = System.Math.Atan2(x, y);
            double c2 = this.homogeneousTransform[2];
            c2 = c2 * c2 + 1e-6;
            r = System.Math.IEEERemainder(r + c2, 2 * c2) - c2 + r * (1 - c2);
            x = r * System.Math.Cos(a);
            y = r * System.Math.Sin(a);
        }

        public override string FunctionName
        {
            get { return "RingsVariation"; }
        }
    }
}
