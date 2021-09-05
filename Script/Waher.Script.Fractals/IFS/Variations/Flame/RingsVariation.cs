using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class RingsVariation : FlameVariationZeroParameters
    {
        public RingsVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            double a = Math.Atan2(x, y);
            double c2 = this.homogeneousTransform[2];
            c2 = c2 * c2 + 1e-6;
            r = Math.IEEERemainder(r + c2, 2 * c2) - c2 + r * (1 - c2);
            x = r * Math.Cos(a);
            y = r * Math.Sin(a);
        }

        public override string FunctionName
        {
            get { return "RingsVariation"; }
        }
    }
}
