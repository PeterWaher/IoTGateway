namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a 8-bit unsigned integer literal.
	/// </summary>
	public class UInt8Literal : SemanticLiteral
	{
		/// <summary>
		/// Represents a 8-bit unsigned integer literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		public UInt8Literal()
			: base()
		{
		}

		/// <summary>
		/// Represents a 8-bit unsigned integer literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		public UInt8Literal(byte Value)
			: base(Value, Value.ToString())
		{
		}

		/// <summary>
		/// Represents a 8-bit unsigned integer literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		/// <param name="StringValue">String value.</param>
		public UInt8Literal(byte Value, string StringValue)
			: base(Value, StringValue)
		{
		}

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType => "http://www.w3.org/2001/XMLSchema#unsignedByte";

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType)
		{
			if (byte.TryParse(Value, out byte i))
				return new UInt8Literal(i, Value);
			else
				return new CustomLiteral(Value, DataType);
		}
	}
}
