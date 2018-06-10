using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class SinusoidalVariation : FlameVariationZeroParameters
    {
        public SinusoidalVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            x = System.Math.Sin(x);
            y = System.Math.Sin(y);
        }

        public override string FunctionName
        {
            get { return "SinusoidalVariation"; }
        }
    }
}
