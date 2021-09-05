using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	public class BlobVariation : FlameVariationMultipleParameters
	{
		private readonly double high;
		private readonly double low;
		private readonly double waves;

		public BlobVariation(ScriptNode high, ScriptNode low, ScriptNode waves, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { high, low, waves }, new ArgumentType[] { ArgumentType.Scalar,
				ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
		{
			this.high = 0;
			this.low = 0;
			this.waves = 0;
		}

		private BlobVariation(double High, double Low, double Waves, ScriptNode high, ScriptNode low, ScriptNode waves,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { high, low, waves }, new ArgumentType[] { ArgumentType.Scalar,
				ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
		{
			this.high = High;
			this.low = Low;
			this.waves = Waves;
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "high", "low", "waves" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			double High = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
			double Low = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
			double Waves = Expression.ToDouble(Arguments[2].AssociatedObjectValue);

			return new BlobVariation(High, Low, Waves, this.Arguments[0], this.Arguments[1], this.Arguments[2],
				this.Start, this.Length, this.Expression);
		}

		public override void Operate(ref double x, ref double y)
		{
			double r = Math.Sqrt(x * x + y * y);
			double a = Math.Atan2(x, y);
			r *= this.low + (this.high - this.low) * 0.5 * (Math.Sin(this.waves * a) + 1);
			x = r * Math.Cos(a);
			y = r * Math.Sin(a);
		}

		public override string FunctionName
		{
			get { return "BlobVariation"; }
		}
	}
}
