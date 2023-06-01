namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a 16-bit integer literal.
    /// </summary>
    public class Int16Literal : SemanticLiteral
    {
        /// <summary>
        /// Represents a 16-bit integer literal.
        /// </summary>
        /// <param name="Value">Literal value</param>
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
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#short";

        /// <summary>
        /// Tries to parse a string value of the type supported by the class..
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <param name="DataType">Data type.</param>
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            if (short.TryParse(Value, out short i))
                return new Int16Literal(i, Value);
            else
                return new CustomLiteral(Value, DataType);
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
