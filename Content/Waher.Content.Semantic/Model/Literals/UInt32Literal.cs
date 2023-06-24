using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a 32-bit unsigned integer literal.
    /// </summary>
    public class UInt32Literal : SemanticNumericLiteral
	{
        /// <summary>
        /// Represents a 32-bit unsigned integer literal.
        /// </summary>
        public UInt32Literal()
            : base()
        {
        }

        /// <summary>
        /// Represents a 32-bit unsigned integer literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public UInt32Literal(uint Value)
            : base(Value, Value.ToString())
        {
        }

        /// <summary>
        /// Represents a 32-bit unsigned integer literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value.</param>
        public UInt32Literal(uint Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#unsignedInt";

		/// <summary>
		/// How well the type supports a given value type.
		/// </summary>
		/// <param name="ValueType">Value Type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(Type ValueType)
		{
			return ValueType == typeof(uint) ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Comparable numeric value.
		/// </summary>
		public override double ComparableValue => (uint)this.Value;

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
        {
            if (uint.TryParse(Value, out uint i))
                return new UInt32Literal(i, Value);
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
			if (Value is uint Typed)
				return new UInt32Literal(Typed);
			else
				return new StringLiteral(Value?.ToString() ?? string.Empty);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is UInt32Literal Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
