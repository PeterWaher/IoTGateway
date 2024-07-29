using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class GaussianVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public GaussianVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double r1, r2, r3, r4, r5;

            lock (this.gen)
            {
                r1 = this.gen.NextDouble();
                r2 = this.gen.NextDouble();
                r3 = this.gen.NextDouble();
                r4 = this.gen.NextDouble();
                r5 = this.gen.NextDouble();
            }

            r1 += r2 + r3 + r4 - 2;
            
            r5 *= Math.PI * 2;
            x = r1 * Math.Cos(r5);
            y = r1 * Math.Sin(r5);
        }

        private readonly Random gen = new Random();

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(GaussianVariation);
    }
}
