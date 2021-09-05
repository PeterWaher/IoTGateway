using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class JuliaVariation : FlameVariationOneParameter
    {
        private readonly double omega;

        public JuliaVariation(ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(Parameter, Start, Length, Expression)
        {
            this.omega = 0;
        }

        private JuliaVariation(double Omega, ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(Parameter, Start, Length, Expression)
        {
            this.omega = Omega;
        }

		public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            return new JuliaVariation(Expression.ToDouble(Argument.AssociatedObjectValue), this.Argument, this.Start, this.Length, this.Expression);
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = Math.Pow(x * x + y * y, 0.25);
            double a = Math.Atan2(x, y) / 2;
            x = Math.Cos(a + this.omega) * r;
            y = Math.Sin(a + this.omega) * r;
        }

        public override string FunctionName
        {
            get { return "JuliaVariation"; }
        }
    }
}
