using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class BladeVariation : FlameVariationZeroParameters
    {
        public BladeVariation(int Start, int Length, Expression Expression)
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

            r1 *= System.Math.Sqrt(x * x + y * y) * this.variationWeight;
            double c = System.Math.Cos(r1);
            double s = System.Math.Sin(r1);
            y = x * (c - s);
            x = x * (c + s);
        }

        private Random gen = new Random();

        public override string FunctionName
        {
            get { return "BladeVariation"; }
        }
    }
}
