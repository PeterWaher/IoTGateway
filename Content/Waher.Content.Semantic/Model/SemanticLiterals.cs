using System;
using Waher.Content.Semantic.Model.Literals;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Set of semantic literals.
	/// </summary>
	public class SemanticLiterals : Ring, IOrderedSet
	{
		/// <summary>
		/// Instance reference to the set of semantic literals.
		/// </summary>
		public static readonly SemanticLiterals Instance = new SemanticLiterals();

		/// <summary>
		/// Set of semantic literals.
		/// </summary>
		public SemanticLiterals()
		{
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return Element is ISemanticLiteral;
		}

		/// <summary>
		/// Size of set, if finite and known, otherwise null is returned.
		/// </summary>
		public override int? Size => null;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj.GetType() == typeof(SemanticLiterals);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return typeof(SemanticLiterals).GetHashCode();
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
		/// If the ring * operator is commutative or not.
		/// </summary>
		public override bool IsCommutative => false;

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero => NullValue.Instance;
	}
}
