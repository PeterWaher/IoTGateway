using System;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a double literal.
	/// </summary>
	public class DoubleLiteral : ISemanticLiteral
	{
		private readonly double value;

		/// <summary>
		/// Represents a double literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		public DoubleLiteral(double Value)
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
		public Type Type => typeof(double);

		/// <summary>
		/// Type name
		/// </summary>
		public string StringType => "http://www.w3.org/2001/XMLSchema#double";

		/// <summary>
		/// String representation of value.
		/// </summary>
		public string StringValue => CommonTypes.Encode(this.value);
	}
}
