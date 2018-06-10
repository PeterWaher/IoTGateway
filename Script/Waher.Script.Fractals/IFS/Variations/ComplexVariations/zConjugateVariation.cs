using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zConjugateVariation : FlameVariationZeroParameters
    {
        public zConjugateVariation(int Start, int Length, Expression Expression)
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