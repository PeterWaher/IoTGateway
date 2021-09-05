using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class NGonVariation : FlameVariationMultipleParameters
    {
        private readonly double power;
        private readonly double sides;
        private readonly double corners;
        private readonly double circle;

        public NGonVariation(ScriptNode high, ScriptNode low, ScriptNode corners, ScriptNode circle,
			int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { high, low, corners, circle },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
            this.power = 0;
            this.sides = 0;
            this.corners = 0;
            this.circle = 0;
        }

		public NGonVariation(double High, double Low, double Corners, double Circle, 
			ScriptNode high, ScriptNode low, ScriptNode corners, ScriptNode circle,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { high, low, corners, circle },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
            this.power = High;
            this.sides = Low;
            this.corners = Corners;
            this.circle = Circle;
        }

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "high", "low", "corners", "circle" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            double High = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            double Low = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
            double Corners = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
            double Circle = Expression.ToDouble(Arguments[3].AssociatedObjectValue);

            return new NGonVariation(High, Low, Corners, Circle, this.Arguments[0],
				this.Arguments[1], this.Arguments[2], this.Arguments[3], this.Start, this.Length,
				this.Expression);
        }

        public override void Operate(ref double x, ref double y)
        {
            double p2 = Math.PI * 2 / this.sides;
            double a = Math.Atan2(y, x);
            double t3 = a - p2 * Math.Floor(a / p2);

            if (t3 <= p2 * 0.5)
                t3 -= p2;

            double r = Math.Sqrt(x * x + y * y);
            double k = (this.corners * (1 / (Math.Cos(t3) + 1e-6) - 1) + this.circle) / Math.Pow(r, this.power);
            x *= k;
            y *= k;
        }

        public override string FunctionName
        {
            get { return "NGonVariation"; }
        }
    }
}
