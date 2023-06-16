using System;
using Waher.Content.Semantic.Model.Literals;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Set of semantic numeric literals.
	/// </summary>
	public class SemanticNumericLiterals : Field, IOrderedSet
	{
		/// <summary>
		/// Instance reference to the set of semantic numeric literals.
		/// </summary>
		public static readonly SemanticNumericLiterals Instance = new SemanticNumericLiterals();

		/// <summary>
		/// Set of semantic literals.
		/// </summary>
		public SemanticNumericLiterals()
		{
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return Element is SemanticNumericLiteral;
		}

		/// <summary>
		/// Size of set, if finite and known, otherwise null is returned.
		/// </summary>
		public override int? Size => null;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj.GetType() == typeof(SemanticNumericLiterals);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return typeof(SemanticNumericLiterals).GetHashCode();
		}

		/// <summary>
		/// Compares two elements.
		/// </summary>
		/// <param name="x">Element 1</param>
		/// <param name="y">Element 2</param>
		/// <returns>Comparison result.</returns>
		public int Compare(IElement x, IElement y)
		{
			if (x is IComparable Left)
				return Left.CompareTo(y);
			else
				return 1;
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero => DoubleLiteral.ZeroInstance;

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public override ICommutativeRingWithIdentityElement One => DoubleLiteral.OneInstance;

	}
}
