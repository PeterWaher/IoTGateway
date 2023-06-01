using System;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents a Uri literal.
    /// </summary>
    public class UriLiteral : SemanticLiteral
    {
        /// <summary>
        /// Represents a Uri literal.
        /// </summary>
        public UriLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents a Uri literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public UriLiteral(Uri Value)
            : base(Value, Value.AbsoluteUri)
        {
        }

        /// <summary>
        /// Represents a Uri literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value</param>
        public UriLiteral(Uri Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#anyURI";

        /// <summary>
        /// Tries to parse a string value of the type supported by the class..
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <param name="DataType">Data type.</param>
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            if (Uri.TryCreate(Value, UriKind.Absolute, out Uri ParsedUri))
                return new UriLiteral(ParsedUri, Value);
            else
                return new CustomLiteral(Value, DataType);
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is UriLiteral Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
