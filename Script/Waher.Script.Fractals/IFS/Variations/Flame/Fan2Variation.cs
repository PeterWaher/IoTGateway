using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class Fan2Variation : FlameVariationMultipleParameters
    {
        private double x;
        private double y;

		public Fan2Variation(ScriptNode x, ScriptNode y, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { x, y }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
            this.x = 0;
            this.y = 0;
        }

		private Fan2Variation(double X, double Y, ScriptNode x, ScriptNode y, int Start, int Length,
			Expression Expression)
			: base(new ScriptNode[] { x, y }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
			this.x = X;
            this.y = Y;
        }

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "x", "y" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            double X = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            double Y = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

            return new Fan2Variation(X, Y, this.Arguments[0], this.Arguments[1], this.Start, this.Length, this.Expression);
        }

        public override void Operate(ref double x, ref double y)
        {
            double r = System.Math.Sqrt(x * x + y * y);
            double a = System.Math.Atan2(x, y);
            double p1 = System.Math.PI * this.x * this.x / 2 + 1e-6;
            double t = a + this.y - 2 * p1 * System.Math.Floor(a * this.y / p1);
            
            if (t > p1)
                a -= p1;
            else
                a += p1;

            x = r * System.Math.Sin(a);
            y = r * System.Math.Cos(a);
        }

        public override string FunctionName
        {
            get { return "Fan2Variation"; }
        }
    }
}
