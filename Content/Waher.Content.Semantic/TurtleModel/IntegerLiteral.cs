using System.Numerics;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents an integer literal of undefined size.
	/// </summary>
	public class IntegerLiteral : SemanticLiteral
	{
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
		/// Type name
		/// </summary>
		public override string StringType => "http://www.w3.org/2001/XMLSchema#integer";

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType)
		{
			if (BigInteger.TryParse(Value, out BigInteger i))
				return new IntegerLiteral(i, Value);
			else
				return new CustomLiteral(Value, DataType);
		}
	}
}
