using System;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a decimal literal.
	/// </summary>
	public class DecimalLiteral : ISemanticLiteral
	{
		private readonly decimal value;

		/// <summary>
		/// Represents a decimal literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		public DecimalLiteral(decimal Value)
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
		public Type Type => typeof(decimal);

		/// <summary>
		/// Type name
		/// </summary>
		public string StringType => "http://www.w3.org/2001/XMLSchema#decimal";

		/// <summary>
		/// String representation of value.
		/// </summary>
		public string StringValue => CommonTypes.Encode(this.value);
	}
}
