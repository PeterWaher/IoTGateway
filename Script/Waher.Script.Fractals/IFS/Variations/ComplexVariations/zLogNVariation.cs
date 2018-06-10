using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zLogNVariation : FlameVariationOneParameter
    {
        private double N;

        public zLogNVariation(ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(Parameter, Start, Length, Expression)
        {
            this.N = 10;
        }

        private zLogNVariation(double N, ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(Parameter, Start, Length, Expression)
        {
            this.N = N;
        }

        public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            return new zLogNVariation(Expression.ToDouble(Argument.AssociatedObjectValue), this.Argument, this.Start, this.Length, this.Expression);
        }

        public override void Operate(ref double x, ref double y)
        {
            double d = 1.0 / System.Math.Log(N);
            // logN(x+iy)
            x = System.Math.Sqrt(x * x + y * y) * d;
            y = System.Math.Atan2(y, x) * d;
        }

        public override string FunctionName
        {
            get { return "zLogNVariation"; }
        }
    }
}