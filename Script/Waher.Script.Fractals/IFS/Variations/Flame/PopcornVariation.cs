using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class PopcornVariation : FlameVariationZeroParameters
    {
        public PopcornVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double x2 = x + this.homogeneousTransform[2] * Math.Sin(Math.Tan(3 * y));
            y += this.homogeneousTransform[5] * Math.Sin(Math.Tan(3 * x));
            x = x2;
        }

        public override string FunctionName
        {
            get { return "PopcornVariation"; }
        }
    }
}
