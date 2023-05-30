using System;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a 64-bit integer literal.
	/// </summary>
	public class Int64Literal : ISemanticLiteral
	{
		private readonly long value;

		/// <summary>
		/// Represents a 64-bit integer literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		public Int64Literal(long Value)
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
		public Type Type => typeof(long);

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
