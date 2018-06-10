using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zLnVariation : FlameVariationZeroParameters
    {
        public zLnVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            // ln(x+iy)
            x = System.Math.Sqrt(x * x + y * y);
            y = System.Math.Atan2(y, x);
        }

        public override string FunctionName
        {
            get { return "zLnVariation"; }
        }
    }
}