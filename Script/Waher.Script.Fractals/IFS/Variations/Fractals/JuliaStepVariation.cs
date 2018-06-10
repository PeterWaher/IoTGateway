using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Fractals
{
    public class JuliaStepVariation : FlameVariationOneComplexParameter
    {
        public JuliaStepVariation(ScriptNode Parameter1, ScriptNode Parameter2, int Start, int Length, Expression Expression)
            : base(Parameter1, Parameter2, Start, Length, Expression)
        {
        }

		public JuliaStepVariation(ScriptNode Parameter1, int Start, int Length, Expression Expression)
			: base(Parameter1, null, Start, Length, Expression)
		{
		}

		private JuliaStepVariation(Complex z, ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(z, Parameter, Start, Length, Expression)
        {
        }

        private JuliaStepVariation(double Re, double Im, ScriptNode Parameter1, ScriptNode Parameter2,
            int Start, int Length, Expression Expression)
            : base(Re, Im, Parameter1, Parameter2, Start, Length, Expression)
        {
        }

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (Arguments[1] == null || Arguments[1].AssociatedObjectValue == null)
			{
				return new JuliaStepVariation(Expression.ToComplex(Arguments[0].AssociatedObjectValue),
					this.Arguments[0], this.Start, this.Length, this.Expression);
			}
			else
			{
				return new JuliaStepVariation(
					Expression.ToDouble(Arguments[0].AssociatedObjectValue), 
					Expression.ToDouble(Arguments[1].AssociatedObjectValue),
					this.Arguments[0], this.Arguments[1], Start, Length, this.Expression);
			}
        }

        public override void Operate(ref double x, ref double y)
        {
            double x2 = x * x - y * y + this.re;
            y = 2 * x * y + this.im;
            x = x2;
        }

        public override string FunctionName
        {
            get { return "JuliaStepVariation"; }
        }
    }
}