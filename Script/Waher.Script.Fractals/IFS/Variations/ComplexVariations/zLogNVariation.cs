using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
	/// <summary>
	/// TODO
	/// </summary>
	public class ZLogNVariation : FlameVariationOneParameter
    {
        private readonly double N;

		/// <summary>
		/// TODO
		/// </summary>
		public ZLogNVariation(ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(Parameter, Start, Length, Expression)
        {
            this.N = 10;
        }

        private ZLogNVariation(double N, ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(Parameter, Start, Length, Expression)
        {
            this.N = N;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            return new ZLogNVariation(Expression.ToDouble(Argument.AssociatedObjectValue), this.Argument, this.Start, this.Length, this.Expression);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double d = 1.0 / Math.Log(N);
            // logN(x+iy)
            x = Math.Sqrt(x * x + y * y) * d;
            y = Math.Atan2(y, x) * d;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(ZLogNVariation);
    }
}