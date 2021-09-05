using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class CrossVariation : FlameVariationZeroParameters
    {
        public CrossVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = Math.Abs(x * x - y * y) + 1e-6;
            x /= r;
            y /= r;
        }

        public override string FunctionName
        {
            get { return "CrossVariation"; }
        }
    }
}
