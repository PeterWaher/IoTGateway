using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class RadialBlurVariation : FlameVariationOneParameter
    {
        private double angle;

		public RadialBlurVariation(ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(Parameter, Start, Length, Expression)
        {
        }

        private RadialBlurVariation(double Angle, ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(Parameter, Start, Length, Expression)
        {
            this.angle = Angle;
        }

        public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            return new RadialBlurVariation(Expression.ToDouble(Argument.AssociatedObjectValue), this.Argument, this.Start, this.Length, this.Expression);
        }

        public override void Operate(ref double x, ref double y)
        {
            double p1 = this.angle * System.Math.PI / 2;
            double r1, r2, r3, r4;

            lock (this.gen)
            {
                r1 = this.gen.NextDouble();
                r2 = this.gen.NextDouble();
                r3 = this.gen.NextDouble();
                r4 = this.gen.NextDouble();
            }

            double t1 = (r1 + r2 + r3 + r4 - 2) / this.variationWeight;
            double t2 = System.Math.Atan2(y, x) + t1 * System.Math.Sin(p1);
            double t3 = t1 * System.Math.Cos(p1) - 1;
            double r = System.Math.Sqrt(x * x + y * y);
            x = (System.Math.Cos(t2) * r + t3 * x) / this.variationWeight;
            y = (System.Math.Sin(t2) * r + t3 * y) / this.variationWeight;
        }

        private Random gen = new Random();

        public override string FunctionName
        {
            get { return "RadialBlurVariation"; }
        }
    }
}
