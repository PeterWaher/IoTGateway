using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class NoiseVariation : FlameVariationZeroParameters
    {
        public NoiseVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            double r1, r2;

            lock (this.gen)
            {
                r1 = this.gen.NextDouble();
                r2 = this.gen.NextDouble();
            }

            r2 *= Math.PI * 2;
            x *= r1 * Math.Cos(r2);
            y *= r1 * Math.Sin(r2);
        }

        private readonly Random gen = new Random();

        public override string FunctionName
        {
            get { return "NoiseVariation"; }
        }
    }
}
