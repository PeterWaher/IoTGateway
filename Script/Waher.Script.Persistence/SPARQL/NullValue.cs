using Waher.Content.Semantic.Model;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Represents a null value.
	/// </summary>
	public class NullValue : SemanticElement
	{
		/// <summary>
		/// Represents a null value.
		/// </summary>
		public NullValue()
		{
		}

		/// <summary>
		/// If element is a literal.
		/// </summary>
		public override bool IsLiteral => true;

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue => null;

		/// <summary>
		/// Compares element to another.
		/// </summary>
		/// <param name="obj">Second element.</param>
		/// <returns>Comparison</returns>
		public override int CompareTo(object obj)
		{
			return obj is NullValue ? 0 : -1;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Empty;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj.GetType() == typeof(NullValue);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.GetType().GetHashCode();
		}
	}
}
