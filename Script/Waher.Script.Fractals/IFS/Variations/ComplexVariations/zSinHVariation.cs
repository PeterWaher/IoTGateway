using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class ZSinHVariation : FlameVariationZeroParameters
    {
        public ZSinHVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double Mod = Math.Exp(x);
            double zr = Mod * (Math.Cos(y));
            double zi = Mod * (Math.Sin(y));

            Mod = Math.Exp(-x);
            zr -= Mod * Math.Cos(-y);
            zi -= Mod * Math.Sin(-y);

            x = zr * 0.5;
            y = zi * 0.5;
        }

        public override string FunctionName
        {
            get { return "zSinHVariation"; }
        }
    }
}