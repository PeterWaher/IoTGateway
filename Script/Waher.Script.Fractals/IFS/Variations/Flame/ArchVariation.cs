using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class ArchVariation : FlameVariationZeroParameters
    {
        public ArchVariation(int Start, int Length, Expression Expression)
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

            r1 *= System.Math.PI * this.variationWeight;
            x = System.Math.Sin(r1);
            y = x * x / (System.Math.Cos(r1) + 1e-6);
        }

        private Random gen = new Random();

        public override string FunctionName
        {
            get { return "ArchVariation"; }
        }
    }
}
