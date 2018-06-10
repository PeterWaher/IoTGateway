using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	public class CurlVariation : FlameVariationMultipleParameters
	{
		private double c1;
		private double c2;

		public CurlVariation(ScriptNode c1, ScriptNode c2, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { c1, c2 }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
			this.c1 = 0;
			this.c2 = 0;
		}

		private CurlVariation(double C1, double C2, ScriptNode c1, ScriptNode c2, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { c1, c2 }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
			this.c1 = C1;
			this.c2 = C2;
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "c1", "c2" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			double C1 = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
			double C2 = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

			return new CurlVariation(C1, C2, this.Arguments[0], this.Arguments[1],
				this.Start, this.Length, this.Expression);
		}

		public override void Operate(ref double x, ref double y)
		{
			double t1 = 1 + this.c1 * x + this.c2 * (x * x - y * y);
			double t2 = this.c1 * y + 2 * this.c2 * x * y;
			double r = t1 * t1 + t2 * t2 + 1e-6;
			x = (x * t1 + y * t2) / r;
			y = (y * t1 - x * t2) / r;
		}

		public override string FunctionName
		{
			get { return "CurlVariation"; }
		}
	}
}
