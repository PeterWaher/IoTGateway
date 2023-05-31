using System.Numerics;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents an integer literal of undefined size.
    /// </summary>
    public class IntegerLiteral : SemanticLiteral
    {
        private readonly string dataType = null;

        /// <summary>
        /// Represents an integer literal of undefined size.
        /// </summary>
        /// <param name="Value">Literal value</param>
        public IntegerLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents an integer literal of undefined size.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public IntegerLiteral(BigInteger Value)
            : base(Value, Value.ToString())
        {
        }

        /// <summary>
        /// Represents an integer literal of undefined size.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value.</param>
        public IntegerLiteral(BigInteger Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Represents an integer literal of undefined size.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value.</param>
        /// <param name="DataType">Data Type.</param>
        public IntegerLiteral(BigInteger Value, string StringValue, string DataType)
            : base(Value, StringValue)
        {
            dataType = DataType;
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => dataType ?? "http://www.w3.org/2001/XMLSchema#integer";

        /// <summary>
        /// How well the type supports a given data type.
        /// </summary>
        /// <param name="DataType">Data type.</param>
        /// <returns>Support grade.</returns>
        public override Grade Supports(string DataType)
        {
            switch (DataType)
            {
                case "http://www.w3.org/2001/XMLSchema#integer":
                case "http://www.w3.org/2001/XMLSchema#negativeInteger":
                case "http://www.w3.org/2001/XMLSchema#nonNegativeInteger":
                case "http://www.w3.org/2001/XMLSchema#nonPositiveInteger":
                case "http://www.w3.org/2001/XMLSchema#positiveInteger":
                    return Grade.Ok;

                default:
                    return Grade.NotAtAll;
            }
        }

        /// <summary>
        /// Tries to parse a string value of the type supported by the class..
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <param name="DataType">Data type.</param>
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            if (BigInteger.TryParse(Value, out BigInteger i))
                return new IntegerLiteral(i, Value, DataType);
            else
                return new CustomLiteral(Value, DataType);
        }
    }
}
