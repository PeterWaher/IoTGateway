namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	/// <summary>
	/// TODO
	/// </summary>
	public class BubbleVariation : FlameVariationZeroParameters
	{
		/// <summary>
		/// TODO
		/// </summary>
		public BubbleVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            double r = 4 / (4 + x * x + y * y);
            x = r * x;
            y = r * y;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(BubbleVariation);
    }
}
