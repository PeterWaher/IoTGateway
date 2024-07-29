using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class WavesVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public WavesVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double t = this.homogeneousTransform[2];
            double x2 = x + this.homogeneousTransform[1] * Math.Sin(y / (t * t + 1e-6));

            t = this.homogeneousTransform[5];
            y += this.homogeneousTransform[4] * Math.Sin(x / (t * t + 1e-6));
            x = x2;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(WavesVariation);
    }
}
