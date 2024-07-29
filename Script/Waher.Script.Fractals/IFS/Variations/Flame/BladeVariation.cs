using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class BladeVariation : FlameVariationZeroParameters
	{
		/// <summary>
		/// TODO
		/// </summary>
		public BladeVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
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

            r1 *= Math.Sqrt(x * x + y * y) * this.variationWeight;
            double c = Math.Cos(r1);
            double s = Math.Sin(r1);
            y = x * (c - s);
            x *= c + s;
        }

        private readonly Random gen = new Random();

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(BladeVariation);
    }
}
