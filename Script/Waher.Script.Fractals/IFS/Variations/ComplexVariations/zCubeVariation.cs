using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class ZCubeVariation : FlameVariationZeroParameters
    {
        public ZCubeVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = x * x - y * y;
            double i = 2 * x * y;

            double x2 = r * x - i * y;
            y = r * y + i * x;
            x = x2;
        }

        public override string FunctionName
        {
            get { return "zCubeVariation"; }
        }
    }
}