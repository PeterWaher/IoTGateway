using System;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;

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
		/// http://www.w3.org/2001/XMLSchema#date
		/// </summary>
		public const string TypeUri = "http://www.w3.org/2001/XMLSchema#date";
		
		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType => TypeUri;

		/// <summary>
		/// How well the type supports a given value type.
		/// </summary>
		/// <param name="ValueType">Value Type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(Type ValueType)
		{
			return Grade.Barely;
		}

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
        {
            if (XML.TryParse(Value, out DateTime TP))
                return new DateLiteral(TP, Value);
            else
                return new CustomLiteral(Value, DataType, Language);
        }

		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		public override ISemanticLiteral Encapsulate(object Value)
		{
			if (Value is DateTime Typed && Typed.TimeOfDay == TimeSpan.Zero)
				return new DateLiteral(Typed);
			else
				return new StringLiteral(Value?.ToString() ?? string.Empty);
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
