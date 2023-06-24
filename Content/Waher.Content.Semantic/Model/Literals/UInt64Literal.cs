using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a 64-bit unsigned integer literal.
    /// </summary>
    public class UInt64Literal : SemanticNumericLiteral
	{
        /// <summary>
        /// Represents a 64-bit unsigned integer literal.
        /// </summary>
        public UInt64Literal()
            : base()
        {
        }

        /// <summary>
        /// Represents a 64-bit unsigned integer literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public UInt64Literal(ulong Value)
            : base(Value, Value.ToString())
        {
        }

        /// <summary>
        /// Represents a 64-bit unsigned integer literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value.</param>
        public UInt64Literal(ulong Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#unsignedLong";

		/// <summary>
		/// How well the type supports a given value type.
		/// </summary>
		/// <param name="ValueType">Value Type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(Type ValueType)
		{
			return ValueType == typeof(ulong) ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		public override ISemanticLiteral Encapsulate(object Value)
		{
			if (Value is ulong Typed)
				return new UInt64Literal(Typed);
			else
				return new StringLiteral(Value?.ToString() ?? string.Empty);
		}

		/// <summary>
		/// Comparable numeric value.
		/// </summary>
		public override double ComparableValue => (ulong)this.Value;

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
        {
            if (ulong.TryParse(Value, out ulong i))
                return new UInt64Literal(i, Value);
            else
                return new CustomLiteral(Value, DataType, Language);
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is UInt64Literal Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
