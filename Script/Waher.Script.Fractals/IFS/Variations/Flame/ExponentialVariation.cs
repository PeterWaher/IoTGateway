using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class ExponentialVariation : FlameVariationZeroParameters
    {
        public ExponentialVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double e = Math.Exp(x - 1);
            y *= Math.PI;
            x = e * Math.Cos(y);
            y = e * Math.Sin(y);
        }

        public override string FunctionName
        {
            get { return "ExponentialVariation"; }
        }
    }
}
