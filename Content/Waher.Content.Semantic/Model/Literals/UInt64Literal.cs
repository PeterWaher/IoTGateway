namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a 64-bit unsigned integer literal.
    /// </summary>
    public class UInt64Literal : SemanticLiteral
    {
        /// <summary>
        /// Represents a 64-bit unsigned integer literal.
        /// </summary>
        /// <param name="Value">Literal value</param>
        public UInt64Literal()
            : base()
        {
        }

        /// <summary>
        /// Represents a 64-bit unsigned integer literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public UInt64Literal(ulong Value)
            : base(Value, Value.ToString())
        {
        }

        /// <summary>
        /// Represents a 64-bit unsigned integer literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value.</param>
        public UInt64Literal(ulong Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#unsignedLong";

        /// <summary>
        /// Tries to parse a string value of the type supported by the class..
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <param name="DataType">Data type.</param>
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            if (ulong.TryParse(Value, out ulong i))
                return new UInt64Literal(i, Value);
            else
                return new CustomLiteral(Value, DataType);
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is UInt64Literal Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
