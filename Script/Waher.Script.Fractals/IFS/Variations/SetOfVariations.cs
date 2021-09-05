using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Fractals.IFS.Variations
{
	public class SetOfVariations : Set
	{
		public static readonly SetOfVariations Instance = new SetOfVariations();

		public SetOfVariations()
		{
		}

		public override bool Contains(IElement Element)
		{
			return
				Element is FlameVariationZeroParameters ||
				Element is FlameVariationOneParameter ||
				Element is FlameVariationOneComplexParameter ||
				Element is FlameVariationMultipleParameters;
		}

		public override bool Equals(object obj)
		{
			return obj is SetOfVariations;
		}

		public override int GetHashCode()
		{
			return ((object)this).GetHashCode();
		}
	}
}
