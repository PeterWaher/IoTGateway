using System;
using Waher.Content.Semantic.Model.Literals;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Abstract base class for semantic literal numeric values.
	/// </summary>
	public abstract class SemanticNumericLiteral : SemanticLiteral, IFieldElement
	{
		/// <summary>
		/// Abstract base class for semantic literal numeric values.
		/// </summary>
		public SemanticNumericLiteral()
		{
		}

		/// <summary>
		/// Abstract base class for semantic literal values.
		/// </summary>
		/// <param name="Value">Parsed Value</param>
		/// <param name="StringValue">String Value</param>
		public SemanticNumericLiteral(object Value, string StringValue)
			: base(Value, StringValue)
		{
		}

		/// <summary>
		/// Comparable numeric value.
		/// </summary>
		public abstract double ComparableValue { get; }

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override IRing AssociatedRing => SemanticNumericLiterals.Instance;

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup => SemanticNumericLiterals.Instance;

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero => DoubleLiteral.ZeroInstance;

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup => SemanticNumericLiterals.Instance;

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup => SemanticNumericLiterals.Instance;

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public ICommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity => SemanticNumericLiterals.Instance;

		/// <summary>
		/// Associated Euclidian Domain.
		/// </summary>
		public IEuclidianDomain AssociatedEuclidianDomain => SemanticNumericLiterals.Instance;

		/// <summary>
		/// Associated Integral Domain.
		/// </summary>
		public IIntegralDomain AssociatedIntegralDomain => SemanticNumericLiterals.Instance;

		/// <summary>
		/// Associated Field.
		/// </summary>
		public IField AssociatedField => SemanticNumericLiterals.Instance;

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public ICommutativeRingWithIdentityElement One => DoubleLiteral.OneInstance;

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public ICommutativeRing AssociatedCommutativeRing => SemanticNumericLiterals.Instance;

		/// <summary>
		/// Compares the current instance with another object of the same type and returns
		/// an integer that indicates whether the current instance precedes, follows, or
		/// occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>A value that indicates the relative order of the objects being compared. The
		/// return value has these meanings: Value Meaning Less than zero This instance precedes
		/// obj in the sort order. Zero This instance occurs in the same position in the
		/// sort order as obj. Greater than zero This instance follows obj in the sort order.</returns>
		/// <exception cref="ArgumentException">obj is not the same type as this instance.</exception>
		public override int CompareTo(object obj)
		{
			if (obj is SemanticNumericLiteral Typed)
				return this.ComparableValue.CompareTo(Typed.ComparableValue);
			else
				return base.CompareTo(obj);
		}

		/// <summary>
		/// Tries to multiply an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IRingElement MultiplyLeft(IRingElement Element)
		{
			if (Element is SemanticNumericLiteral Typed)
				return new DoubleLiteral(Typed.ComparableValue * this.ComparableValue);
			else
				return null;
		}

		/// <summary>
		/// Tries to multiply an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IRingElement MultiplyRight(IRingElement Element)
		{
			if (Element is SemanticNumericLiteral Typed)
				return new DoubleLiteral(this.ComparableValue * Typed.ComparableValue);
			else
				return null;
		}

		/// <summary>
		/// Tries to multiply an element to the current element.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual ICommutativeRingElement Multiply(ICommutativeRingElement Element)
		{
			if (Element is SemanticNumericLiteral Typed)
				return new DoubleLiteral(Typed.ComparableValue * this.ComparableValue);
			else
				return null;
		}

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		public override IRingElement Invert()
		{
			double d = this.ComparableValue;
			if (d == 0)
				return null;
			else
				return new DoubleLiteral(1.0 / d);
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			if (Element is SemanticNumericLiteral Typed)
				return new DoubleLiteral(this.ComparableValue + Typed.ComparableValue);
			else
				return null;
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return new DoubleLiteral(-this.ComparableValue);
		}

		/// <summary>
		/// Tries to add an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ISemiGroupElement AddLeft(ISemiGroupElement Element)
		{
			if (Element is SemanticNumericLiteral Typed)
				return new DoubleLiteral(Typed.ComparableValue + this.ComparableValue);
			else
				return null;
		}

		/// <summary>
		/// Tries to add an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ISemiGroupElement AddRight(ISemiGroupElement Element)
		{
			if (Element is SemanticNumericLiteral Typed)
				return new DoubleLiteral(this.ComparableValue + Typed.ComparableValue);
			else
				return null;
		}
	}
}
