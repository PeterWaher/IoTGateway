using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class TangentVariation : FlameVariationZeroParameters
    {
        public TangentVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double cy = System.Math.Cos(y) + 1e-6;
            x = System.Math.Sin(x) / cy;
            y = System.Math.Sin(y) / cy;
        }

        public override string FunctionName
        {
            get { return "TangentVariation"; }
        }
    }
}
