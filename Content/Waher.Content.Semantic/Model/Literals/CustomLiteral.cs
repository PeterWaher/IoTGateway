using System.Text;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a custom literal.
    /// </summary>
    public class CustomLiteral : SemanticLiteral
    {
        /// <summary>
        /// Represents a custom literal.
        /// </summary>
        /// <param name="Value">Literal value</param>
        /// <param name="Type">Data type.</param>
        public CustomLiteral(string Value, string Type)
            : base(Value, Value)
        {
            this.StringType = Type;
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType { get; }

        /// <summary>
        /// Tries to parse a string value of the type supported by the class..
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <param name="DataType">Data type.</param>
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            return new CustomLiteral(Value, DataType);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('"');
            sb.Append(JSON.Encode(this.StringValue));
            sb.Append("\"^^<");
            sb.Append(this.StringType);
            sb.Append('>');

            return sb.ToString();
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is CustomLiteral Typed &&
				Typed.StringValue == this.StringValue &&
                Typed.StringType == this.StringType;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.StringValue.GetHashCode();
			Result ^= Result << 5 ^ this.StringType.GetHashCode();
            return Result;
		}
	}
}
