using System;
using System.Text;
using Waher.Content.Semantic.Ontologies;
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
		/// http://www.w3.org/2001/XMLSchema#string
		/// </summary>
		public const string TypeUri = XmlSchema.Namespace + "string";

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
            return string.IsNullOrEmpty(DataType) || DataType == TypeUri ? Grade.Ok : Grade.NotAtAll;
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

		/// <summary>
		/// Compares the current instance with another object of the same type and returns
		/// an integer that indicates whether the current instance precedes, follows, or
		/// occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>A value that indicates the relative order of the objects being compared. The
		/// return value has these meanings: Value Meaning Less than zero This instance precedes
		/// obj in the sort order. Zero This instance occurs in the same position in the
		/// sort order as obj. Greater than zero This instance follows obj in the sort order.</returns>
		/// <exception cref="ArgumentException">obj is not the same type as this instance.</exception>
		public override int CompareTo(object obj)
		{
			if (obj is StringLiteral Typed)
			{
				int i = this.StringValue.CompareTo(Typed.StringValue);
				if (i != 0)
					return i;

				return this.language?.CompareTo(Typed.language) ?? (Typed.language is null ? 0 : -1);
			}

			return base.CompareTo(obj);
		}

	}
}
