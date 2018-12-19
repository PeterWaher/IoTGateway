using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zPowerExponentVariation : FlameVariationOneComplexParameter
    {
        public zPowerExponentVariation(ScriptNode Parameter1, ScriptNode Parameter2, int Start, int Length, Expression Expression)
            : base(Parameter1, Parameter2, Start, Length, Expression)
        {
        }

		public zPowerExponentVariation(ScriptNode Parameter1, int Start, int Length, Expression Expression)
			: base(Parameter1, null, Start, Length, Expression)
		{
		}

		private zPowerExponentVariation(Complex z, ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(z, Parameter, Start, Length, Expression)
        {
        }

        private zPowerExponentVariation(double Re, double Im, ScriptNode Parameter1, ScriptNode Parameter2,
			int Start, int Length, Expression Expression)
            : base(Re, Im, Parameter1, Parameter2, Start, Length, Expression)
        {
        }

        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
			if (Arguments[1] is null || Arguments[1].AssociatedObjectValue is null)
				return new zPowerExponentVariation(Expression.ToComplex(Arguments[0].AssociatedObjectValue), this.Arguments[0], this.Start, this.Length, this.Expression);
            else
            {
                return new zPowerExponentVariation(Expression.ToDouble(Arguments[0].AssociatedObjectValue), Expression.ToDouble(Arguments[1].AssociatedObjectValue),
                    this.Arguments[0], this.Arguments[1], this.Start, this.Length, this.Expression);
            }
        }

        public override void Operate(ref double x, ref double y)
        {
            // (x+iy)^z

            double lnz = System.Math.Log(System.Math.Sqrt(x * x + y * y));
            double argz = System.Math.Atan2(y, x);
            double amp = System.Math.Exp(this.re * lnz - this.im * argz);
            double phi = this.im * lnz + this.re * argz;

            x = amp * System.Math.Cos(phi);
            y = amp * System.Math.Sin(phi);
        }

        public override string FunctionName
        {
            get { return "zPowerExponentVariation"; }
        }
    }
}