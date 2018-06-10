using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class FanVariation : FlameVariationZeroParameters
    {
        public FanVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y);
            double a = System.Math.Atan2(x, y);
            double t = this.homogeneousTransform[2];
            t = System.Math.PI * t * t + 1e-6;

            if (System.Math.IEEERemainder(a + this.homogeneousTransform[5], t) > t / 2)
                a -= t / 2;
            else
                a += t / 2;

            x = r * System.Math.Cos(a);
            y = r * System.Math.Sin(a);
        }

        public override string FunctionName
        {
            get { return "FanVariation"; }
        }
    }
}
