using System;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a 32-bit integer literal.
	/// </summary>
	public class Int32Literal : ISemanticLiteral
	{
		private readonly int value;

		/// <summary>
		/// Represents a 32-bit integer literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		public Int32Literal(int Value)
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
		public Type Type => typeof(int);

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
