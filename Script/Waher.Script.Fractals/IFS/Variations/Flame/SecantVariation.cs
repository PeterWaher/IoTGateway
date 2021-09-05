using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class SecantVariation : FlameVariationZeroParameters
    {
        public SecantVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            y = 1 / (this.variationWeight * Math.Cos(this.variationWeight * r));
        }

        public override string FunctionName
        {
            get { return "SecantVariation"; }
        }
    }
}
