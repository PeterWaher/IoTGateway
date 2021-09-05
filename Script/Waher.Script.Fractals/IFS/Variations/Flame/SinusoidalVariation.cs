using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class SinusoidalVariation : FlameVariationZeroParameters
    {
        public SinusoidalVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            x = Math.Sin(x);
            y = Math.Sin(y);
        }

        public override string FunctionName
        {
            get { return "SinusoidalVariation"; }
        }
    }
}
