using Waher.Content.Semantic;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Represents a null value.
	/// </summary>
	public class NullValue : ISemanticElement
	{
		/// <summary>
		/// Represents a null value.
		/// </summary>
		public NullValue()
		{
		}

		/// <summary>
		/// Property used by processor, to tag information to an element.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// If element is a literal.
		/// </summary>
		public bool IsLiteral => true;

		/// <summary>
		/// Underlying element value represented by the semantic element.
		/// </summary>
		public object ElementValue => null;

		/// <summary>
		/// Compares element to another.
		/// </summary>
		/// <param name="obj">Second element.</param>
		/// <returns>Comparison</returns>
		public int CompareTo(object obj)
		{
			return obj is NullValue ? 0 : -1;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Empty;
		}
	}
}
