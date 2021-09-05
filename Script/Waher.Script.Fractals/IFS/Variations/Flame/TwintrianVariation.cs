using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class TwintrianVariation : FlameVariationZeroParameters
    {
        public TwintrianVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r1;

            lock (this.gen)
            {
                r1 = this.gen.NextDouble();
            }

            r1 *= Math.Sqrt(x * x + y * y) * this.variationWeight;

            double s = Math.Sin(r1);
            double c = Math.Cos(r1);
            double t = Math.Log10(s * s) + c;

            y = x * (t - Math.PI * s);
            x *= t;
        }

        private readonly Random gen = new Random();

        public override string FunctionName
        {
            get { return "TwintrianVariation"; }
        }
    }
}
