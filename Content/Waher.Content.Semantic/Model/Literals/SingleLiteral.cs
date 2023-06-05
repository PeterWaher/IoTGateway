namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a float literal.
    /// </summary>
    public class SingleLiteral : SemanticNumericLiteral
	{
        /// <summary>
        /// Represents a float literal.
        /// </summary>
        public SingleLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents a float literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public SingleLiteral(float Value)
            : base(Value, CommonTypes.Encode(Value))
        {
        }

        /// <summary>
        /// Represents a float literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value</param>
        public SingleLiteral(float Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#float";

		/// <summary>
		/// Comparable numeric value.
		/// </summary>
		public override double ComparableValue => (float)this.Value;

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
        {
            if (CommonTypes.TryParse(Value, out float d))
                return new SingleLiteral(d, Value);
            else
                return new CustomLiteral(Value, DataType, Language);
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is SingleLiteral Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
