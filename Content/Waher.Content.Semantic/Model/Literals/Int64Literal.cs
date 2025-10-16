﻿using System;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a 64-bit integer literal.
    /// </summary>
    public class Int64Literal : SemanticNumericLiteral
	{
        /// <summary>
        /// Represents a 64-bit integer literal.
        /// </summary>
        public Int64Literal()
            : base()
        {
        }

        /// <summary>
        /// Represents a 64-bit integer literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public Int64Literal(long Value)
            : base(Value, Value.ToString())
        {
        }

        /// <summary>
        /// Represents a 64-bit integer literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value.</param>
        public Int64Literal(long Value, string StringValue)
            : base(Value, StringValue)
        {
        }

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#long
		/// </summary>
		public static readonly string TypeUri = XmlSchema.@long.OriginalString;

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
			return ValueType == typeof(long) ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Comparable numeric value.
		/// </summary>
		public override double ComparableValue => (long)this.Value;

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
        {
            if (long.TryParse(Value, out long i))
                return new Int64Literal(i, Value);
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
			if (Value is long Typed)
				return new Int64Literal(Typed);
			else
				return new StringLiteral(Value?.ToString() ?? string.Empty);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is Int64Literal Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
