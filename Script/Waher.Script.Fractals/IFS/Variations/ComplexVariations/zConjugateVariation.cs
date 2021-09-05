using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class ZConjugateVariation : FlameVariationZeroParameters
    {
        public ZConjugateVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            y = -y;
        }

        public override string FunctionName
        {
            get { return "zConjugateVariation"; }
        }
    }
}