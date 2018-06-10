using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class CylinderVariation : FlameVariationZeroParameters
    {
        public CylinderVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            x = System.Math.Sin(x);
        }

        public override string FunctionName
        {
            get { return "CylinderVariation"; }
        }
    }
}
