using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class JuliaVariation : FlameVariationOneParameter
    {
        private double omega;

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
            double r = System.Math.Pow(x * x + y * y, 0.25);
            double a = System.Math.Atan2(x, y) / 2;
            x = System.Math.Cos(a + this.omega) * r;
            y = System.Math.Sin(a + this.omega) * r;
        }

        public override string FunctionName
        {
            get { return "JuliaVariation"; }
        }
    }
}
