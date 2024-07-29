using System;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class CylinderVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public CylinderVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            x = Math.Sin(x);
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(CylinderVariation);
    }
}
