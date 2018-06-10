using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class ExponentialVariation : FlameVariationZeroParameters
    {
        public ExponentialVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double e = System.Math.Exp(x - 1);
            y *= System.Math.PI;
            x = e * System.Math.Cos(y);
            y = e * System.Math.Sin(y);
        }

        public override string FunctionName
        {
            get { return "ExponentialVariation"; }
        }
    }
}
