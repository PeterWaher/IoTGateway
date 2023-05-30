namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a bool literal.
	/// </summary>
	public class BooleanLiteral : SemanticLiteral
	{
		/// <summary>
		/// Predefined value "true".
		/// </summary>
		public readonly static BooleanLiteral True = new BooleanLiteral(true, "true");

		/// <summary>
		/// Predefined value "false".
		/// </summary>
		public readonly static BooleanLiteral False = new BooleanLiteral(false, "false");

		/// <summary>
		/// Represents a bool literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		public BooleanLiteral()
			: base()
		{
		}

		/// <summary>
		/// Represents a bool literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		public BooleanLiteral(bool Value)
			: base(Value, CommonTypes.Encode(Value))
		{
		}

		/// <summary>
		/// Represents a bool literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		/// <param name="StringValue">String value.</param>
		public BooleanLiteral(bool Value, string StringValue)
			: base(Value, StringValue)
		{
		}

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType => "http://www.w3.org/2001/XMLSchema#boolean";

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType)
		{
			if (CommonTypes.TryParse(Value, out bool b))
				return new BooleanLiteral(b, Value);
			else
				return new CustomLiteral(Value, DataType);
		}
	}
}
