using System;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Abstract base class for semantic literal numeric values.
	/// </summary>
	public abstract class SemanticNumericLiteral : SemanticLiteral
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
	}
}
