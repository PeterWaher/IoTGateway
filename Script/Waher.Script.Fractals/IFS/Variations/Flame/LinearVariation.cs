using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class LinearVariation : FlameVariationZeroParameters
    {
        public LinearVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            // do nothing;
        }

        public override string FunctionName
        {
            get { return "LinearVariation"; }
        }
    }
}
