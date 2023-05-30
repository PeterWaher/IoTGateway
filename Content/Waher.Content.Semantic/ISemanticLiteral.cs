using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for semantic literals.
	/// </summary>
	public interface ISemanticLiteral : ISemanticElement, IProcessingSupport<string>
	{
		/// <summary>
		/// Parsed value.
		/// </summary>
		object Value { get; }

		/// <summary>
		/// Type name (or null if literal value is a string)
		/// </summary>
		string StringType { get; }

		/// <summary>
		/// String representation of value.
		/// </summary>
		string StringValue { get; }

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <returns>Parsed literal.</returns>
		ISemanticLiteral Parse(string Value, string DataType);
	}
}
