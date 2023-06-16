using System;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for semantic literals.
	/// </summary>
	public interface ISemanticLiteral : ISemanticElement, IProcessingSupport<string>, 
		IProcessingSupport<Type>, IRingElement
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
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		ISemanticLiteral Parse(string Value, string DataType, string Language);

		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		ISemanticLiteral Encapsulate(object Value);
	}
}
