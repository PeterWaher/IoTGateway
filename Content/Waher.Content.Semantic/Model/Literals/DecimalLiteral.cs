namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a decimal literal.
    /// </summary>
    public class DecimalLiteral : SemanticLiteral
    {
        /// <summary>
        /// Represents a decimal literal.
        /// </summary>
        public DecimalLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents a decimal literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public DecimalLiteral(decimal Value)
            : base(Value, CommonTypes.Encode(Value))
        {
        }

        /// <summary>
        /// Represents a decimal literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value</param>
        public DecimalLiteral(decimal Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#decimal";

        /// <summary>
        /// Tries to parse a string value of the type supported by the class..
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <param name="DataType">Data type.</param>
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            if (CommonTypes.TryParse(Value, out decimal d))
                return new DecimalLiteral(d, Value);
            else
                return new CustomLiteral(Value, DataType);
        }
    }
}
