using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class ZLnVariation : FlameVariationZeroParameters
    {
        public ZLnVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            // ln(x+iy)
            x = Math.Sqrt(x * x + y * y);
            y = Math.Atan2(y, x);
        }

        public override string FunctionName
        {
            get { return "zLnVariation"; }
        }
    }
}