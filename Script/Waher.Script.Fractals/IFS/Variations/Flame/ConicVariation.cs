using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	public class ConicVariation : FlameVariationMultipleParameters
	{
		private double holes;
		private double eccentricity;

		public ConicVariation(ScriptNode holes, ScriptNode eccentricity, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { holes, eccentricity }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
			this.holes = 0;
			this.eccentricity = 0;
		}

		private ConicVariation(double Holes, double Eccentricity,
			ScriptNode holes, ScriptNode eccentricity, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { holes, eccentricity }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
			this.holes = Holes;
			this.eccentricity = Eccentricity;
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "holes", "eccentricity" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			double Holes = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
			double Eccentricity = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

			return new ConicVariation(Holes, Eccentricity, this.Arguments[0], this.Arguments[1], 
				this.Start, this.Length, this.Expression);
		}

		public override void Operate(ref double x, ref double y)
		{
			double r1;

			lock (this.gen)
			{
				r1 = this.gen.NextDouble();
			}

			double a = System.Math.Atan2(y, x);
			double r = (r1 - this.holes) * this.eccentricity / (1 + this.eccentricity + System.Math.Cos(a));
			x = r * System.Math.Cos(a);
			y = r * System.Math.Sin(a);
		}

		private Random gen = new Random();

		public override string FunctionName
		{
			get { return "ConicVariation"; }
		}
	}
}
