using System;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a custom literal.
    /// </summary>
    public class CustomLiteral : SemanticLiteral
    {
		private readonly string language;

		/// <summary>
		/// Represents a custom literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		/// <param name="Type">Data type.</param>
		public CustomLiteral(string Value, string Type)
            : base(Value, Value)
        {
            this.StringType = Type;
			this.language = null;
		}

		/// <summary>
		/// Represents a custom literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		/// <param name="Type">Data type.</param>
		/// <param name="Language">Language of string.</param>
		public CustomLiteral(string Value, string Type, string Language)
			: base(Value, Value)
		{
			this.StringType = Type;
			this.language = Language;
		}

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType { get; }

		/// <summary>
		/// Language of string.
		/// </summary>
		public string Language => this.language;

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
            return new CustomLiteral(Value, DataType, Language);
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

			sb.Append("^^<");
            sb.Append(this.StringType);
            sb.Append('>');

            return sb.ToString();
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is CustomLiteral Typed &&
				Typed.StringValue == this.StringValue &&
                Typed.StringType == this.StringType &&
				Typed.language == this.language;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.StringValue.GetHashCode();
			Result ^= Result << 5 ^ this.StringType.GetHashCode();
			Result ^= Result << 5 ^ (this.language?.GetHashCode() ?? 0);
			return Result;
		}
	}
}
