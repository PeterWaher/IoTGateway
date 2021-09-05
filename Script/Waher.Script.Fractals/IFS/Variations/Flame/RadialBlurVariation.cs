using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class RadialBlurVariation : FlameVariationOneParameter
    {
        private readonly double angle;

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
            double p1 = this.angle * Math.PI / 2;
            double r1, r2, r3, r4;

            lock (this.gen)
            {
                r1 = this.gen.NextDouble();
                r2 = this.gen.NextDouble();
                r3 = this.gen.NextDouble();
                r4 = this.gen.NextDouble();
            }

            double t1 = (r1 + r2 + r3 + r4 - 2) / this.variationWeight;
            double t2 = Math.Atan2(y, x) + t1 * Math.Sin(p1);
            double t3 = t1 * Math.Cos(p1) - 1;
            double r = Math.Sqrt(x * x + y * y);
            x = (Math.Cos(t2) * r + t3 * x) / this.variationWeight;
            y = (Math.Sin(t2) * r + t3 * y) / this.variationWeight;
        }

        private readonly Random gen = new Random();

        public override string FunctionName
        {
            get { return "RadialBlurVariation"; }
        }
    }
}
