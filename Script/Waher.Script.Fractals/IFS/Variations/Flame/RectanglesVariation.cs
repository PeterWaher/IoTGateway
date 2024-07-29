using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class RectanglesVariation : FlameVariationMultipleParameters
    {
        private readonly double x;
        private readonly double y;

		/// <summary>
		/// TODO
		/// </summary>
		public RectanglesVariation(ScriptNode x, ScriptNode y, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { x, y }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
			this.x = 0;
            this.y = 0;
        }

		private RectanglesVariation(double X, double Y, ScriptNode x, ScriptNode y, int Start, int Length,
			Expression Expression)
			: base(new ScriptNode[] { x, y }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
            this.x = X;
            this.y = Y;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "x", "y" };
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            double X = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            double Y = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

            return new RectanglesVariation(X, Y, this.Arguments[0], this.Arguments[1], this.Start, this.Length, this.Expression);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            x = (2 * Math.Floor(x / this.x) + 1) * this.x - x;
            y = (2 * Math.Floor(y / this.y) + 1) * this.y - y;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(RectanglesVariation);
    }
}
