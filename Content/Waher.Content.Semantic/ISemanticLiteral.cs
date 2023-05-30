using System;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for semantic literals.
	/// </summary>
	public interface ISemanticLiteral : ISemanticElement
	{
		/// <summary>
		/// Parsed value.
		/// </summary>
		object Value { get; }

		/// <summary>
		/// Type of value.
		/// </summary>
		Type Type { get; }

		/// <summary>
		/// Type name (or null if literal value is a string)
		/// </summary>
		string StringType { get; }

		/// <summary>
		/// String representation of value.
		/// </summary>
		string StringValue { get; }
	}
}
