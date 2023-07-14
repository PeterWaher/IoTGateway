using System;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Inventory;

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
		/// http://www.w3.org/2001/XMLSchema#time
		/// </summary>
		public const string TypeUri = XmlSchema.Namespace + "time";

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
			return ValueType == typeof(TimeSpan) ? Grade.Ok : Grade.NotAtAll;
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
            if (TimeSpan.TryParse(Value, out TimeSpan TP))
                return new TimeLiteral(TP, Value);
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
			if (Value is TimeSpan Typed)
				return new TimeLiteral(Typed);
			else
				return new StringLiteral(Value?.ToString() ?? string.Empty);
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
