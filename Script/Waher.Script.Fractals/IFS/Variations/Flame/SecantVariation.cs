using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class SecantVariation : FlameVariationZeroParameters
    {
        public SecantVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y);
            y = 1 / (this.variationWeight * System.Math.Cos(this.variationWeight * r));
        }

        public override string FunctionName
        {
            get { return "SecantVariation"; }
        }
    }
}
