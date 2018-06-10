using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class BubbleVariation : FlameVariationZeroParameters
    {
        public BubbleVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = 4 / (4 + x * x + y * y);
            x = r * x;
            y = r * y;
        }

        public override string FunctionName
        {
            get { return "BubbleVariation"; }
        }
    }
}
