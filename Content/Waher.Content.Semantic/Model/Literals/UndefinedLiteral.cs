using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
	/// <summary>
	/// Represents an undefined literal.
	/// </summary>
	public class UndefinedLiteral : SemanticLiteral
	{
		/// <summary>
		/// Represents an undefined literal.
		/// </summary>
		public UndefinedLiteral()
			: base()
		{
		}

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType => null;

		/// <summary>
		/// How well the type supports a given value type.
		/// </summary>
		/// <param name="ValueType">Value Type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(Type ValueType)
		{
			return Grade.NotAtAll;
		}

		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		public override ISemanticLiteral Encapsulate(object Value)
		{
			return new UndefinedLiteral();
		}

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
		{
			return new UndefinedLiteral();
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is UndefinedLiteral;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return typeof(UndefinedLiteral).GetHashCode();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return "UNDEF";
		}
	}
}
