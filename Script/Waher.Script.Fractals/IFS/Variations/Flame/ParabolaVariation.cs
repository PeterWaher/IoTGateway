using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class ParabolaVariation : FlameVariationMultipleParameters
    {
        private readonly double width;
        private readonly double height;

		/// <summary>
		/// TODO
		/// </summary>
		public ParabolaVariation(ScriptNode width, ScriptNode height, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { width, height }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
            this.width = 0;
            this.height = 0;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public ParabolaVariation(double Width, double Height, ScriptNode width, ScriptNode height, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { width, height }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
        {
            this.width = Width;
            this.height = Height;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "width", "height" };
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            double Width = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            double Height = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

            return new ParabolaVariation(Width, Height, this.Arguments[0], this.Arguments[1], this.Start, this.Length, this.Expression);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double r1;

            lock (this.gen)
            {
                r1 = this.gen.NextDouble();
            }

            double r = Math.Sqrt(x * x + y * y);
            double s = Math.Sin(r);
            double c = Math.Cos(r);
            x = this.height * s * s * r1;
            y = this.width * c * r1;
        }

        private readonly Random gen = new Random();

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(ParabolaVariation);
    }
}
