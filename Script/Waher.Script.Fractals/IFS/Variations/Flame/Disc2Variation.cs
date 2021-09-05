using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class Disc2Variation : FlameVariationMultipleParameters
    {
        private readonly double rotation;
        private readonly double twist;

        public Disc2Variation(ScriptNode rotation, ScriptNode twist, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { rotation, twist }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
			this.rotation = 0;
            this.twist = 0;
        }

        private Disc2Variation(double Rotation, double Twist, ScriptNode rotation, ScriptNode twist, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { rotation, twist }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
			this.rotation = Rotation;
            this.twist = Twist;
        }

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "rotation", "twist" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            double Rotation = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            double Twist = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

            return new Disc2Variation(Rotation, Twist, this.Arguments[0], this.Arguments[1],
				this.Start, this.Length, this.Expression);
        }

        public override void Operate(ref double x, ref double y)
        {
            double d2rtp = this.rotation * Math.PI;
            double s0 = Math.Sin(this.twist);
            double c0 = Math.Cos(this.twist);

            if (this.twist > pi2)
            {
                double k = 1 + this.twist - pi2;
                s0 *= k;
                c0 *= k;
            }

            if (this.twist < -pi2)
            {
                double k = 1 + this.twist + pi2;
                s0 *= k;
                c0 *= k;

            }

            double t = d2rtp * (x + y);
            double sinr = Math.Sin(t);
            double cosr = Math.Cos(t);
            double r = Math.Atan2(x, y) / Math.PI;
            x = (sinr + c0) * r;
            y = (cosr + s0) * r;
        }

        private const double pi2 = 2 * Math.PI;

        public override string FunctionName
        {
            get { return "Disc2Variation"; }
        }
    }
}
