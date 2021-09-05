using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class PolarVariation : FlameVariationZeroParameters
    {
        public PolarVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            double a = Math.Atan2(x, y);
            x = a / Math.PI;
            y = (r - 1);
        }

        public override string FunctionName
        {
            get { return "PolarVariation"; }
        }
    }
}
