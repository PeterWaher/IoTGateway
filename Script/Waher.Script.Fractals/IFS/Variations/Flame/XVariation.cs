using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class XVariation : FlameVariationZeroParameters
    {
        public XVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y);
            double a = System.Math.Atan2(x, y);
            double p0 = System.Math.Sin(a + r);
            double p1 = System.Math.Cos(a - r);
            p0 = p0 * p0 * p0;
            p1 = p1 * p1 * p1;
            x = r * (p0 + p1);
            y = r * (p0 - p1);
        }

        public override string FunctionName
        {
            get { return "XVariation"; }
        }
    }
}
