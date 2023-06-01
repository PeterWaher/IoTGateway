using System;
using Waher.Content.Xml;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a date literal.
    /// </summary>
    public class DateLiteral : SemanticLiteral
    {
        /// <summary>
        /// Represents a date literal.
        /// </summary>
        public DateLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents a date literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public DateLiteral(DateTime Value)
            : base(Value, XML.Encode(Value, true))
        {
        }

        /// <summary>
        /// Represents a date literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value</param>
        public DateLiteral(DateTime Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#date";

        /// <summary>
        /// Tries to parse a string value of the type supported by the class..
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <param name="DataType">Data type.</param>
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            if (XML.TryParse(Value, out DateTime TP))
                return new DateLiteral(TP, Value);
            else
                return new CustomLiteral(Value, DataType);
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is DateLiteral Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
