namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a double literal.
    /// </summary>
    public class DoubleLiteral : SemanticNumericLiteral
	{
        /// <summary>
        /// Represents a double literal.
        /// </summary>
        public DoubleLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents a double literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public DoubleLiteral(double Value)
            : base(Value, CommonTypes.Encode(Value))
        {
        }

        /// <summary>
        /// Represents a double literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value</param>
        public DoubleLiteral(double Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#double";

		/// <summary>
		/// Comparable numeric value.
		/// </summary>
		public override double ComparableValue => (double)this.Value;

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
        {
            if (CommonTypes.TryParse(Value, out double d))
                return new DoubleLiteral(d, Value);
            else
                return new CustomLiteral(Value, DataType, Language);
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is DoubleLiteral Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
