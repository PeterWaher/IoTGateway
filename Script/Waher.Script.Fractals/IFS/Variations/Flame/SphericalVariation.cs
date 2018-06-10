using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class SphericalVariation : FlameVariationZeroParameters
    {
        public SphericalVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = x * x + y * y + 1e-6;
            x /= r;
            y /= r;
        }

        public override string FunctionName
        {
            get { return "SphericalVariation"; }
        }
    }
}
