using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class ZSqrtVariation : FlameVariationZeroParameters
    {
        public ZSqrtVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            // (x+iy)^0.5
            double argz = Math.Atan2(y, x);
            double amp = Math.Pow(x * x + y * y, 0.25);
            double phi = 0.5 * argz;

            x = amp * Math.Cos(phi);
            y = amp * Math.Sin(phi);
        }

        public override string FunctionName
        {
            get { return "zSqrtVariation"; }
        }
    }
}