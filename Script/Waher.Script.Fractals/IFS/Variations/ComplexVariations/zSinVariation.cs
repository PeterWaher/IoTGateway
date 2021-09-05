using System;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class ZSinVariation : FlameVariationZeroParameters
    {
        public ZSinVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double Mod = Math.Exp(-y);
            double zr = Mod * (Math.Cos(x));
            double zi = Mod * (Math.Sin(x));

            Mod = Math.Exp(y);
            zr -= Mod * Math.Cos(-x);
            zi -= Mod * Math.Sin(-x);

            x = zr * 0.5;
            y = zi * 0.5;
        }

        public override string FunctionName
        {
            get { return "zSinVariation"; }
        }
    }
}