using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Fractals
{
    public class JuliaRoot2Variation : FlameVariationOneComplexParameter
    {
        public JuliaRoot2Variation(ScriptNode Parameter1, ScriptNode Parameter2, int Start, int Length, Expression Expression)
            : base(Parameter1, Parameter2, Start, Length, Expression)
        {
        }

		public JuliaRoot2Variation(ScriptNode Parameter1, int Start, int Length, Expression Expression)
			: base(Parameter1, null, Start, Length, Expression)
		{
		}

		private JuliaRoot2Variation(Complex z, ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(z, Parameter, Start, Length, Expression)
        {
        }

        private JuliaRoot2Variation(double Re, double Im, ScriptNode Parameter1, ScriptNode Parameter2,
			int Start, int Length, Expression Expression)
            : base(Re, Im, Parameter1, Parameter2, Start, Length, Expression)
        {
        }

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (Arguments[1] == null || Arguments[1].AssociatedObjectValue == null)
			{
				return new JuliaRoot2Variation(Expression.ToComplex(Arguments[0].AssociatedObjectValue),
					this.Arguments[0], this.Start, this.Length, this.Expression);
			}
			else
			{
				return new JuliaRoot2Variation(
					Expression.ToDouble(Arguments[0].AssociatedObjectValue),
					Expression.ToDouble(Arguments[1].AssociatedObjectValue),
					this.Arguments[0], this.Arguments[1], Start, Length, this.Expression);
			}
		}

        public override void Operate(ref double x, ref double y)
        {
            // -sqrt(x+iy-z)

            double re = x - this.re;
            double im = y - this.im;

            double argz = System.Math.Atan2(im, re);
            double amp = -System.Math.Pow(re * re + im * im, 0.25);
            double phi = 0.5 * argz;

            x = amp * System.Math.Cos(phi);
            y = amp * System.Math.Sin(phi);
        }

        public override string FunctionName
        {
            get { return "JuliaRoot2Variation"; }
        }
    }
}