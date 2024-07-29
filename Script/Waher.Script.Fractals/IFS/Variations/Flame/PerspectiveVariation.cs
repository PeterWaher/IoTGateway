using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class PerspectiveVariation : FlameVariationMultipleParameters
    {
        private readonly double angle;
        private readonly double distance;

		/// <summary>
		/// TODO
		/// </summary>
		public PerspectiveVariation(ScriptNode angle, ScriptNode distance, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { angle, distance }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
            this.angle = 0;
            this.distance = 0;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public PerspectiveVariation(double Angle, double Distance, ScriptNode angle, ScriptNode distance, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { angle, distance }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
            this.angle = Angle;
            this.distance = Distance;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "angle", "distance" };
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            double Angle = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            double Distance = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

            return new PerspectiveVariation(Angle, Distance, this.Arguments[0], this.Arguments[1], this.Start, this.Length, this.Expression);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double d = this.distance / (this.distance - y * Math.Sin(this.angle));
            x *= d;
            y *= d * Math.Cos(this.angle);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(PerspectiveVariation);
    }
}
