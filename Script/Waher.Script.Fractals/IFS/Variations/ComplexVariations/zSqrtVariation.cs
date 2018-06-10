using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zSqrtVariation : FlameVariationZeroParameters
    {
        public zSqrtVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            // (x+iy)^0.5
            double argz = System.Math.Atan2(y, x);
            double amp = System.Math.Pow(x * x + y * y, 0.25);
            double phi = 0.5 * argz;

            x = amp * System.Math.Cos(phi);
            y = amp * System.Math.Sin(phi);
        }

        public override string FunctionName
        {
            get { return "zSqrtVariation"; }
        }
    }
}