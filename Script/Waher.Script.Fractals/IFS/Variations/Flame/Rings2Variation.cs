using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class Rings2Variation : FlameVariationOneParameter
    {
        private readonly double value;
		/// <summary>
		/// TODO
		/// </summary>
		public Rings2Variation(ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(Parameter, Start, Length, Expression)
        {
        }

        private Rings2Variation(double Value, ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(Parameter, Start, Length, Expression)
        {
            this.value = Value;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            return new Rings2Variation(Expression.ToDouble(Argument.AssociatedObjectValue), this.Argument, this.Start, this.Length, this.Expression);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double p = this.value * this.value + 1e-6;
            double r = Math.Pow(x * x + y * y, 0.25);
            double a = Math.Atan2(x, y) / 2;
            double t = r - 2 * p * Math.Floor((r + p) / (2 * p)) + r * (1 - p);
            x = t * Math.Sin(a);
            y = t * Math.Cos(a);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(Rings2Variation);
    }
}
