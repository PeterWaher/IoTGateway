using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class CosineVariation : FlameVariationZeroParameters
    {
        public CosineVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            x *= System.Math.PI;
            double x2 = System.Math.Cos(x) * System.Math.Cosh(y);
            y = -System.Math.Sin(x) * System.Math.Sinh(y);
            x = x2;
        }

        public override string FunctionName
        {
            get { return "CosineVariation"; }
        }
    }
}
