using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class PolarVariation : FlameVariationZeroParameters
    {
        public PolarVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y);
            double a = System.Math.Atan2(x, y);
            x = a / System.Math.PI;
            y = (r - 1);
        }

        public override string FunctionName
        {
            get { return "PolarVariation"; }
        }
    }
}
