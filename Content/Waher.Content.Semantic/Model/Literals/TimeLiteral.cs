using System;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a time literal.
    /// </summary>
    public class TimeSpanLiteral : SemanticLiteral
    {
        /// <summary>
        /// Represents a time literal.
        /// </summary>
        public TimeSpanLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents a time literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public TimeSpanLiteral(TimeSpan Value)
            : base(Value, Value.ToString())
        {
        }

        /// <summary>
        /// Represents a time literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value</param>
        public TimeSpanLiteral(TimeSpan Value, string StringValue)
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
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            if (TimeSpan.TryParse(Value, out TimeSpan TP))
                return new TimeSpanLiteral(TP, Value);
            else
                return new CustomLiteral(Value, DataType);
        }
    }
}
