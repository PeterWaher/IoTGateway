using System;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a time literal.
    /// </summary>
    public class TimeLiteral : SemanticLiteral
    {
        /// <summary>
        /// Represents a time literal.
        /// </summary>
        public TimeLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents a time literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public TimeLiteral(TimeSpan Value)
            : base(Value, Value.ToString())
        {
        }

        /// <summary>
        /// Represents a time literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value</param>
        public TimeLiteral(TimeSpan Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#time";

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
        {
            if (TimeSpan.TryParse(Value, out TimeSpan TP))
                return new TimeLiteral(TP, Value);
            else
                return new CustomLiteral(Value, DataType, Language);
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is TimeLiteral Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
