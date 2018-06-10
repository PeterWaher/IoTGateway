using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class SquareVariation : FlameVariationZeroParameters
    {
        public SquareVariation(int Start, int Length, Expression Expression)
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

            x = r1 - 0.5;
            y = r2 - 0.5;
        }

        private Random gen = new Random();

        public override string FunctionName
        {
            get { return "SquareVariation"; }
        }
    }
}
