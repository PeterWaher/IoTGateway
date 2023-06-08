using System;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a string literal.
    /// </summary>
    public class StringLiteral : SemanticLiteral
    {
        private readonly string language;

        /// <summary>
        /// Represents a string literal.
        /// </summary>
        public StringLiteral()
            : base()
        {
            this.language = null;
        }

        /// <summary>
        /// Represents a string literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public StringLiteral(string Value)
            : base(Value, Value)
        {
			this.language = null;
        }

        /// <summary>
        /// Represents a string literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="Language">Language of string.</param>
        public StringLiteral(string Value, string Language)
            : base(Value, Value)
        {
			this.language = Language;
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => string.Empty;

		/// <summary>
		/// How well the type supports a given value type.
		/// </summary>
		/// <param name="ValueType">Value Type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(Type ValueType)
		{
			return ValueType == typeof(string) ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Language of string.
		/// </summary>
		public string Language => this.language;

        /// <summary>
        /// How well the type supports a given data type.
        /// </summary>
        /// <param name="DataType">Data type.</param>
        /// <returns>Support grade.</returns>
        public override Grade Supports(string DataType)
        {
            return string.IsNullOrEmpty(DataType) || DataType == "http://www.w3.org/2001/XMLSchema#string" ? Grade.Ok : Grade.NotAtAll;
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
            return new StringLiteral(Value, Language);
        }

		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		public override ISemanticLiteral Encapsulate(object Value)
		{
			return new StringLiteral(Value?.ToString() ?? string.Empty);
		}

		/// <inheritdoc/>
		public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('"');
            sb.Append(JSON.Encode(this.StringValue));
            sb.Append('"');

            if (!string.IsNullOrEmpty(this.language))
            {
                sb.Append('@');
                sb.Append(this.language);
            }

            return sb.ToString();
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is StringLiteral Typed &&
				Typed.StringValue == this.StringValue &&
                string.Compare(Typed.language, this.language, true) == 0;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.StringValue.GetHashCode();
			Result ^= Result << 5 ^ (this.language?.GetHashCode() ?? 0);
            return Result;
		}
	}
}
