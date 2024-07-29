using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class ArchVariation : FlameVariationZeroParameters
	{
		/// <summary>
		/// TODO
		/// </summary>
		public ArchVariation(int Start, int Length, Expression Expression)
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

            r1 *= Math.PI * this.variationWeight;
            x = Math.Sin(r1);
            y = x * x / (Math.Cos(r1) + 1e-6);
        }

        private readonly Random gen = new Random();

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(ArchVariation);
    }
}
