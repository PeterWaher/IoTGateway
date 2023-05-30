using System;
using System.Numerics;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents an integer literal of undefined size.
	/// </summary>
	public class BigIntegerLiteral : ISemanticLiteral
	{
		private readonly BigInteger value;

		/// <summary>
		/// Represents an integer literal of undefined size.
		/// </summary>
		/// <param name="Value">Literal value</param>
		public BigIntegerLiteral(BigInteger Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Parsed value.
		/// </summary>
		public object Value => this.value;

		/// <summary>
		/// Type of value.
		/// </summary>
		public Type Type => typeof(BigInteger);

		/// <summary>
		/// Type name
		/// </summary>
		public string StringType => "http://www.w3.org/2001/XMLSchema#integer";

		/// <summary>
		/// String representation of value.
		/// </summary>
		public string StringValue => this.value.ToString();
	}
}
