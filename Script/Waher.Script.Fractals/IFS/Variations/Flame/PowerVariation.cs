using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class PowerVariation : FlameVariationZeroParameters
    {
        public PowerVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            double a = Math.Atan2(x, y);
            double s = Math.Sin(a);
            r = Math.Pow(r, s);
            x = r * Math.Cos(a);
            y = r * s;
        }

        public override string FunctionName
        {
            get { return "PowerVariation"; }
        }
    }
}
