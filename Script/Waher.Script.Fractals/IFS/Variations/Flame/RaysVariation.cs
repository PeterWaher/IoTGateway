using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class RaysVariation : FlameVariationZeroParameters
    {
        public RaysVariation(int Start, int Length, Expression Expression)
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

            double r = x * x + y * y + 1e-6;
            r = this.variationWeight * Math.Tan(r1 * Math.PI * this.variationWeight) / r;
            x = r * Math.Cos(x);
            y = r * Math.Cos(y);
        }

        private readonly Random gen = new Random();

        public override string FunctionName
        {
            get { return "RaysVariation"; }
        }
    }
}
