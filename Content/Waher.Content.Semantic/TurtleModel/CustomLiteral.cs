using System;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a custom literal.
	/// </summary>
	public class CustomLiteral : ISemanticLiteral
	{
		private readonly string value;
		private readonly string type;

		/// <summary>
		/// Represents a custom literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		/// <param name="Type">Data type.</param>
		public CustomLiteral(string Value, string Type)
		{
			this.value = Value;
			this.type = Type;
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
		public string StringType => this.type;

		/// <summary>
		/// String representation of value.
		/// </summary>
		public string StringValue => this.value;
	}
}
