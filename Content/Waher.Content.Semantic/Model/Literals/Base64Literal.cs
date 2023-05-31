using System;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a byte[] literal.
    /// </summary>
    public class Base64Literal : SemanticLiteral
    {
        /// <summary>
        /// Represents a byte[] literal.
        /// </summary>
        /// <param name="Value">Literal value</param>
        public Base64Literal()
            : base()
        {
        }

        /// <summary>
        /// Represents a byte[] literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public Base64Literal(byte[] Value)
            : base(Value, Convert.ToBase64String(Value))
        {
        }

        /// <summary>
        /// Represents a byte[] literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value.</param>
        public Base64Literal(byte[] Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#base64Binary";

        /// <summary>
        /// Tries to parse a string value of the type supported by the class..
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <param name="DataType">Data type.</param>
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            try
            {
                return new Base64Literal(Convert.FromBase64String(Value), Value);
            }
            catch (Exception)
            {
                return new CustomLiteral(Value, DataType);
            }
        }
    }
}
