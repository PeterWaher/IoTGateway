using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class SuperShapeVariation : FlameVariationMultipleParameters
    {
        private readonly double m;
        private readonly double n1;
        private readonly double n2;
        private readonly double n3;
        private readonly double holes;
        private readonly double rnd;

		public SuperShapeVariation(ScriptNode m, ScriptNode n1, ScriptNode n2, ScriptNode n3,
			ScriptNode holes, ScriptNode rnd, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { m, n1, n2, n3, holes, rnd },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
					  ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
            this.m = 0;
            this.n1 = 0;
            this.n2 = 0;
            this.n3 = 0;
            this.holes = 0;
            this.rnd = 0;
        }

		public SuperShapeVariation(double M, double N1, double N2, double N3,
			double Holes, double Rnd, ScriptNode m, ScriptNode n1, ScriptNode n2, ScriptNode n3,
			ScriptNode holes, ScriptNode rnd, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { m, n1, n2, n3, holes, rnd },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
					  ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
            this.m = M;
            this.n1 = N1;
            this.n2 = N2;
            this.n3 = N3;
            this.holes = Holes;
            this.rnd = Rnd;
        }

        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            double M = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            double N1 = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
            double N2 = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
            double N3 = Expression.ToDouble(Arguments[3].AssociatedObjectValue);
            double Holes = Expression.ToDouble(Arguments[4].AssociatedObjectValue);
            double Rnd = Expression.ToDouble(Arguments[5].AssociatedObjectValue);

            return new SuperShapeVariation(M, N1, N2, N3, Holes, Rnd, this.Arguments[0],
				this.Arguments[1], this.Arguments[2], this.Arguments[3], this.Arguments[4],
				this.Arguments[5], this.Start, this.Length, this.Expression);
        }

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "m", "n1", "n2", "n3", "holes", "rnd" };
			}
		}

        public override void Operate(ref double x, ref double y)
        {
            double r1;

            lock (this.gen)
            {
                r1 = this.gen.NextDouble();
            }

            double pneg1_n1 = -1.0 / this.n1;
            double theta = (this.m * Math.Atan2(y, x) + Math.PI) / 4.0;
            double t1 = Math.Pow(Math.Abs(Math.Sin(theta)), this.n2);
            double t2 = Math.Pow(Math.Abs(Math.Cos(theta)), this.n3);
            double r = Math.Sqrt(x * x + y * y) + 1e-6;
            r = ((this.rnd * r1 + (1.0 - this.rnd) * r - this.holes) * Math.Pow(t1 + t2, pneg1_n1)) / r;
            x *= r;
            y *= r;
        }

        private readonly Random gen = new Random();

        public override string FunctionName
        {
            get { return "SuperShapeVariation"; }
        }
    }
}
