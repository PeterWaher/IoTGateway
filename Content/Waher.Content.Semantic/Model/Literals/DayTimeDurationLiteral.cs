using System;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a DayTimeDuration literal.
    /// </summary>
    public class DayTimeDurationLiteral : SemanticLiteral
    {
        /// <summary>
        /// Represents a DayTimeDuration literal.
        /// </summary>
        public DayTimeDurationLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents a DayTimeDuration literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public DayTimeDurationLiteral(Duration Value)
            : base(Value, Value.ToString())
        {
        }

        /// <summary>
        /// Represents a DayTimeDuration literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value</param>
        public DayTimeDurationLiteral(Duration Value, string StringValue)
            : base(Value, StringValue)
        {
        }

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#dayTimeDuration
		/// </summary>
		public static readonly string TypeUri = XmlSchema.dayTimeDuration.OriginalString;

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
			return ValueType == typeof(Duration) ? Grade.Barely : Grade.NotAtAll;
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
            if (Duration.TryParse(Value, out Duration d))
                return new DayTimeDurationLiteral(d, Value);
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
			if (Value is Duration Typed)
				return new DayTimeDurationLiteral(Typed);
			else
				return new StringLiteral(Value?.ToString() ?? string.Empty);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is DayTimeDurationLiteral Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
