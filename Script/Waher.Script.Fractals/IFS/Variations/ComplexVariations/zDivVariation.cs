using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zDivVariation : FlameVariationOneComplexParameter
    {
        public zDivVariation(ScriptNode Parameter1, ScriptNode Parameter2, int Start, int Length, Expression Expression)
            : base(Parameter1, Parameter2, Start, Length, Expression)
        {
        }

		public zDivVariation(ScriptNode Parameter1, int Start, int Length, Expression Expression)
			: base(Parameter1, null, Start, Length, Expression)
		{
		}

		private zDivVariation(Complex z, ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(z, Parameter, Start, Length, Expression)
        {
        }

        private zDivVariation(double Re, double Im, ScriptNode Parameter1, ScriptNode Parameter2,
			int Start, int Length, Expression Expression)
            : base(Re, Im, Parameter1, Parameter2, Start, Length, Expression)
        {
        }
		
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
			if (Arguments[1] == null || Arguments[1].AssociatedObjectValue == null)
				return new zDivVariation(Expression.ToComplex(Arguments[0].AssociatedObjectValue), this.Arguments[0], this.Start, this.Length, this.Expression);
            else
            {
                return new zDivVariation(Expression.ToDouble(Arguments[0].AssociatedObjectValue), 
					Expression.ToDouble(Arguments[1].AssociatedObjectValue),
                    this.Arguments[0], this.Arguments[1], this.Start, this.Length, this.Expression);
            }
        }

        public override void Operate(ref double x, ref double y)
        {
            // z/(x+iy)=(re+iim)*(x-iy)/(x^2+y^2)=(re*x+im*y)/r^2+i*(im*x-re*y)/r^2

            double r = x * x + y * y + 1e-6;
            double x2 = (x * this.re + y * this.im) / r;
            y = this.im * x - this.re * y;
            x = x2;
        }

        public override string FunctionName
        {
            get { return "zDivVariation"; }
        }
    }
}