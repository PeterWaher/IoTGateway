using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

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

            r2 *= System.Math.PI * 2;
            x *= r1 * System.Math.Cos(r2);
            y *= r1 * System.Math.Sin(r2);
        }

        private Random gen = new Random();

        public override string FunctionName
        {
            get { return "NoiseVariation"; }
        }
    }
}
