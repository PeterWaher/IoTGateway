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
            language = null;
        }

        /// <summary>
        /// Represents a string literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public StringLiteral(string Value)
            : base(Value, Value)
        {
            language = null;
        }

        /// <summary>
        /// Represents a string literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="Language">Language of string.</param>
        public StringLiteral(string Value, string Language)
            : base(Value, Value)
        {
            language = Language;
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => string.Empty;

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
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            return new StringLiteral(Value);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('"');
            sb.Append(JSON.Encode(StringValue));
            sb.Append('"');

            if (!string.IsNullOrEmpty(language))
            {
                sb.Append('@');
                sb.Append(language);
            }

            return sb.ToString();
        }
    }
}
