using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class SwirlVariation : FlameVariationZeroParameters
    {
        public SwirlVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = x * x + y * y;
            double s = Math.Sin(r);
            double c = Math.Cos(r);
            double x2 = x * s - y * c;
            y = x * c + y * s;
            x = x2;
        }

        public override string FunctionName
        {
            get { return "SwirlVariation"; }
        }
    }
}
