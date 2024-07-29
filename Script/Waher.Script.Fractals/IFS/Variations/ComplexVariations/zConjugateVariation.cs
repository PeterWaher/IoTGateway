namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
	/// <summary>
	/// TODO
	/// </summary>
	public class ZConjugateVariation : FlameVariationZeroParameters
    {
		/// <summary>
		/// TODO
		/// </summary>
		public ZConjugateVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override void Operate(ref double x, ref double y)
        {
            y = -y;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string FunctionName => nameof(ZConjugateVariation);
    }
}