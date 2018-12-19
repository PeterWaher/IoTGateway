using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zMulVariation : FlameVariationOneComplexParameter
    {
        public zMulVariation(ScriptNode Parameter1, ScriptNode Parameter2, int Start, int Length, Expression Expression)
            : base(Parameter1, Parameter2, Start, Length, Expression)
        {
        }

		public zMulVariation(ScriptNode Parameter1, int Start, int Length, Expression Expression)
			: base(Parameter1, null, Start, Length, Expression)
		{
		}

		private zMulVariation(Complex z, ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(z, Parameter, Start, Length, Expression)
        {
        }

        private zMulVariation(double Re, double Im, ScriptNode Parameter1, ScriptNode Parameter2,
			int Start, int Length, Expression Expression)
            : base(Re, Im, Parameter1, Parameter2, Start, Length, Expression)
        {
        }

        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
			if (Arguments[1] is null || Arguments[1].AssociatedObjectValue is null)
				return new zMulVariation(Expression.ToComplex(Arguments[0].AssociatedObjectValue), this.Arguments[0], this.Start, this.Length, this.Expression);
            else
            {
                return new zMulVariation(Expression.ToDouble(Arguments[0].AssociatedObjectValue), Expression.ToDouble(Arguments[1].AssociatedObjectValue),
                    this.Arguments[0], this.Arguments[1], this.Start, this.Length, this.Expression);
            }
        }

        public override void Operate(ref double x, ref double y)
        {
            // z*(x+iy)
            double x2 = x * this.re - y * this.im;
            y = this.re * y + this.im * x;
            x = x2;
        }

        public override string FunctionName
        {
            get { return "zMulVariation"; }
        }
    }
}