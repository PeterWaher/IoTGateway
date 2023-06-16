using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
	/// <summary>
	/// Represents a null value.
	/// </summary>
	public class NullValue : SemanticLiteral
	{
		/// <summary>
		/// Null-value (Note: Do not reuse, except in instances where Tag object will not be used.)
		/// </summary>
		internal static readonly NullValue Instance = new NullValue();

		/// <summary>
		/// Represents a null value.
		/// </summary>
		public NullValue()
			: base(null, string.Empty)
		{
		}

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue => null;

		/// <summary>
		/// Type name (or null if literal value is a string)
		/// </summary>
		public override string StringType => string.Empty;

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

		/// <summary>
		/// How well the type supports a given data type.
		/// </summary>
		/// <param name="DataType">Data type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(string DataType)
		{
			return Grade.NotAtAll;
		}

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
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
		{
			if (Value is null)
				return new NullValue();
			else
				return new CustomLiteral(Value, DataType, Language);
		}

		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		public override ISemanticLiteral Encapsulate(object Value)
		{
			if (Value is null)
				return new NullValue();
			else if (SemanticElements.Encapsulate(Value) is ISemanticLiteral Literal)
				return Literal;
			else
				throw new ArgumentException("Not a literal.", nameof(Value));
		}
	}
}
