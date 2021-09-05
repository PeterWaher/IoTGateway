using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class DiscVariation : FlameVariationZeroParameters
    {
        public DiscVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            double a = Math.Atan2(x, y);
            x = a * Math.Sin(r * Math.PI) / Math.PI;
            y = a * Math.Cos(r * Math.PI) / Math.PI;
        }

        public override string FunctionName
        {
            get { return "DiscVariation"; }
        }
    }
}
