namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a 64-bit integer literal.
	/// </summary>
	public class Int64Literal : SemanticLiteral
	{
		/// <summary>
		/// Represents a 64-bit integer literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		public Int64Literal()
			: base()
		{
		}

		/// <summary>
		/// Represents a 64-bit integer literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		public Int64Literal(long Value)
			: base(Value, Value.ToString())
		{
		}

		/// <summary>
		/// Represents a 64-bit integer literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		/// <param name="StringValue">String value.</param>
		public Int64Literal(long Value, string StringValue)
			: base(Value, StringValue)
		{
		}

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType => "http://www.w3.org/2001/XMLSchema#long";

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType)
		{
			if (long.TryParse(Value, out long i))
				return new Int64Literal(i, Value);
			else
				return new CustomLiteral(Value, DataType);
		}
	}
}
