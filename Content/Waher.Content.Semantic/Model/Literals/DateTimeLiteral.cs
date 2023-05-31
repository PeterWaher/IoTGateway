using System;
using Waher.Content.Xml;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a dateTime literal.
    /// </summary>
    public class DateTimeLiteral : SemanticLiteral
    {
        /// <summary>
        /// Represents a dateTime literal.
        /// </summary>
        public DateTimeLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents a dateTime literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public DateTimeLiteral(DateTimeOffset Value)
            : base(Value, XML.Encode(Value))
        {
        }

        /// <summary>
        /// Represents a dateTime literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value</param>
        public DateTimeLiteral(DateTimeOffset Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#dateTime";

        /// <summary>
        /// Tries to parse a string value of the type supported by the class..
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <param name="DataType">Data type.</param>
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            if (XML.TryParse(Value, out DateTimeOffset TP))
                return new DateTimeLiteral(TP, Value);
            else
                return new CustomLiteral(Value, DataType);
        }
    }
}
