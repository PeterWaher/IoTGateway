namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class BentVariation : FlameVariationZeroParameters
	{
		/// <summary>
		/// TODO
		/// </summary>
		public BentVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            if (x < 0)
                x *= 2;

            if (y < 0)
                y *= 0.5;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(BentVariation);
    }
}
