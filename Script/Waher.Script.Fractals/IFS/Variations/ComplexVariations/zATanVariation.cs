using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class ZATanVariation : FlameVariationZeroParameters
    {
        public ZATanVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double zr = 1 + y;
            double zi = -x;

            double zr2 = 1 - y;
            double zi2 = x;

            double d = 1.0 / (zr2 * zr2 + zi2 * zi2);

            x = (zr * zr2 + zi * zi2) * d;
            y = (zi * zr2 - zr * zi2) * d;
        }

        public override string FunctionName
        {
            get { return "zATanVariation"; }
        }
    }
}