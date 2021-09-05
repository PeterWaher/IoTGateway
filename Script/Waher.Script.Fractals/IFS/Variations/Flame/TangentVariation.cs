using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class TangentVariation : FlameVariationZeroParameters
    {
        public TangentVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double cy = Math.Cos(y) + 1e-6;
            x = Math.Sin(x) / cy;
            y = Math.Sin(y) / cy;
        }

        public override string FunctionName
        {
            get { return "TangentVariation"; }
        }
    }
}
