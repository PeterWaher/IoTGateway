using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class CosineVariation : FlameVariationZeroParameters
    {
        public CosineVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            x *= Math.PI;
            double x2 = Math.Cos(x) * Math.Cosh(y);
            y = -Math.Sin(x) * Math.Sinh(y);
            x = x2;
        }

        public override string FunctionName
        {
            get { return "CosineVariation"; }
        }
    }
}
