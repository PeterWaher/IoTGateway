using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class GaussianVariation : FlameVariationZeroParameters
    {
        public GaussianVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r1, r2, r3, r4, r5;

            lock (this.gen)
            {
                r1 = this.gen.NextDouble();
                r2 = this.gen.NextDouble();
                r3 = this.gen.NextDouble();
                r4 = this.gen.NextDouble();
                r5 = this.gen.NextDouble();
            }

            r1 += r2 + r3 + r4 - 2;
            
            r5 *= Math.PI * 2;
            x = r1 * Math.Cos(r5);
            y = r1 * Math.Sin(r5);
        }

        private readonly Random gen = new Random();

        public override string FunctionName
        {
            get { return "GaussianVariation"; }
        }
    }
}
