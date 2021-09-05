using System;

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
            x = Math.Sin(x);
        }

        public override string FunctionName
        {
            get { return "CylinderVariation"; }
        }
    }
}
