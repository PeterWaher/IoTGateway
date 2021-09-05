using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class ZExpVariation : FlameVariationZeroParameters
    {
        public ZExpVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            // e^*(x+iy)
            double Mod = Math.Exp(x);
            x = Mod * Math.Cos(y);
            y = Mod * Math.Sin(y);
        }

        public override string FunctionName
        {
            get { return "zExpVariation"; }
        }
    }
}