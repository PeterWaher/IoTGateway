using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Fractals.IFS.Variations
{
	/// <summary>
	/// TODO
	/// </summary>
	public class SetOfVariations : Set
	{
		/// <summary>
		/// TODO
		/// </summary>
		public static readonly SetOfVariations Instance = new SetOfVariations();

		/// <summary>
		/// TODO
		/// </summary>
		public SetOfVariations()
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override bool Contains(IElement Element)
		{
			return
				Element is FlameVariationZeroParameters ||
				Element is FlameVariationOneParameter ||
				Element is FlameVariationOneComplexParameter ||
				Element is FlameVariationMultipleParameters;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is SetOfVariations;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override int GetHashCode()
		{
			return ((object)this).GetHashCode();
		}
	}
}
