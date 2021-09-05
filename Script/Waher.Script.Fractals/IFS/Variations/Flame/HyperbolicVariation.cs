using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class HyperbolicVariation : FlameVariationZeroParameters
    {
        public HyperbolicVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y) + 1e-6;
            double a = Math.Atan2(x, y);
            x = Math.Sin(a) / r;
            y = r * Math.Cos(a);
        }

        public override string FunctionName
        {
            get { return "HyperbolicVariation"; }
        }
    }
}
