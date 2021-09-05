using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class PdjVariation : FlameVariationMultipleParameters
    {
        private readonly double a;
        private readonly double b;
        private readonly double c;
        private readonly double d;

		public PdjVariation(ScriptNode a, ScriptNode b, ScriptNode c, ScriptNode d,
			int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { a, b, c, d},
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
			this.a = 0;
            this.b = 0;
            this.c = 0;
            this.d = 0;
        }

		public PdjVariation(double A, double B, double C, double D, 
			ScriptNode a, ScriptNode b, ScriptNode c, ScriptNode d,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { a, b, c, d },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
            this.a = A;
            this.b = B;
            this.c = C;
            this.d = D;
        }

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "a", "b", "c", "d" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            double A = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            double B = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
            double C = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
            double D = Expression.ToDouble(Arguments[3].AssociatedObjectValue);

            return new PdjVariation(A, B, C, D, this.Arguments[0],
				this.Arguments[1], this.Arguments[2], this.Arguments[3], this.Start, this.Length,
				this.Expression);
        }

        public override void Operate(ref double x, ref double y)
        {
            x = Math.Sin(this.a * y) - Math.Cos(this.b * x);
            y = Math.Sin(this.c * x) - Math.Cos(this.d * y);
        }

        public override string FunctionName
        {
            get { return "PdjVariation"; }
        }
    }
}
