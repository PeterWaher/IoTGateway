using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class SpiralVariation : FlameVariationZeroParameters
    {
        public SpiralVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y) + 1e-6;
            double a = Math.Atan2(x, y);
            x = (Math.Cos(a) + Math.Sin(r)) / r;
            y = (Math.Sin(a) - Math.Cos(r)) / r;
        }

        public override string FunctionName
        {
            get { return "SpiralVariation"; }
        }
    }
}
