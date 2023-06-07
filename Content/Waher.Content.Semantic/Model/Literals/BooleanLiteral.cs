﻿namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a bool literal.
    /// </summary>
    public class BooleanLiteral : SemanticLiteral
    {
        /// <summary>
        /// Represents a bool literal.
        /// </summary>
        /// <param name="Value">Literal value</param>
        public BooleanLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents a bool literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public BooleanLiteral(bool Value)
            : base(Value, CommonTypes.Encode(Value))
        {
        }

        /// <summary>
        /// Represents a bool literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value.</param>
        public BooleanLiteral(bool Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#boolean";

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
        {
            if (CommonTypes.TryParse(Value, out bool b))
                return new BooleanLiteral(b, Value);
            else
                return new CustomLiteral(Value, DataType, Language);
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is BooleanLiteral Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.Value.GetHashCode();
		}
	}
}