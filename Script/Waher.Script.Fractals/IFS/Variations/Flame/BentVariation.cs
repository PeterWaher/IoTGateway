using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class BentVariation : FlameVariationZeroParameters
    {
        public BentVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            if (x < 0)
                x *= 2;

            if (y < 0)
                y *= 0.5;
        }

        public override string FunctionName
        {
            get { return "BentVariation"; }
        }
    }
}
