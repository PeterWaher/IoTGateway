using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class HeartVariation : FlameVariationZeroParameters
    {
        public HeartVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            double a = Math.Atan2(x, y);
            x = r * Math.Sin(r * a);
            y = -r * Math.Cos(r * a);
        }

        public override string FunctionName
        {
            get { return "HeartVariation"; }
        }
    }
}
