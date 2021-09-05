using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class DiamondVariation : FlameVariationZeroParameters
    {
        public DiamondVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            double a = Math.Atan2(x, y);
            x = Math.Sin(a) * Math.Cos(r);
            y = Math.Cos(a) * Math.Sin(r);
        }

        public override string FunctionName
        {
            get { return "DiamondVariation"; }
        }
    }
}
