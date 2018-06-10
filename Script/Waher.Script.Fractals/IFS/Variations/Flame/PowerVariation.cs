using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class PowerVariation : FlameVariationZeroParameters
    {
        public PowerVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y);
            double a = System.Math.Atan2(x, y);
            double s = System.Math.Sin(a);
            r = System.Math.Pow(r, s);
            x = r * System.Math.Cos(a);
            y = r * s;
        }

        public override string FunctionName
        {
            get { return "PowerVariation"; }
        }
    }
}
