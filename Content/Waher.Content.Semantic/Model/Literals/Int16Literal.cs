using System;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a 16-bit integer literal.
    /// </summary>
    public class Int16Literal : SemanticNumericLiteral
	{
        /// <summary>
        /// Represents a 16-bit integer literal.
        /// </summary>
        public Int16Literal()
            : base()
        {
        }

        /// <summary>
        /// Represents a 16-bit integer literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public Int16Literal(short Value)
            : base(Value, Value.ToString())
        {
        }

        /// <summary>
        /// Represents a 16-bit integer literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value.</param>
        public Int16Literal(short Value, string StringValue)
            : base(Value, StringValue)
        {
        }

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#short
		/// </summary>
		public static readonly string TypeUri = XmlSchema.@short.OriginalString;

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType => TypeUri;

		/// <summary>
		/// How well the type supports a given value type.
		/// </summary>
		/// <param name="ValueType">Value Type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(Type ValueType)
		{
			return ValueType == typeof(short) ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Comparable numeric value.
		/// </summary>
		public override double ComparableValue => (short)this.Value;

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
        {
            if (short.TryParse(Value, out short i))
                return new Int16Literal(i, Value);
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
			if (Value is short Typed)
				return new Int16Literal(Typed);
			else
				return new StringLiteral(Value?.ToString() ?? string.Empty);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is Int16Literal Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
