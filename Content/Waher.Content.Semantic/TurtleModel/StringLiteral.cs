using System;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a string literal.
	/// </summary>
	public class StringLiteral : ISemanticLiteral
	{
		private readonly string value;

		/// <summary>
		/// Represents a string literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		public StringLiteral(string Value)
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
		public Type Type => typeof(string);

		/// <summary>
		/// Type name
		/// </summary>
		public string StringType => null;

		/// <summary>
		/// String representation of value.
		/// </summary>
		public string StringValue => this.value;
	}
}
