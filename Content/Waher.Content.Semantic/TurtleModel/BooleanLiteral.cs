using System;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a bool literal.
	/// </summary>
	public class BooleanLiteral : ISemanticLiteral
	{
		/// <summary>
		/// Predefined value "true".
		/// </summary>
		public readonly static BooleanLiteral True = new BooleanLiteral(true);

		/// <summary>
		/// Predefined value "false".
		/// </summary>
		public readonly static BooleanLiteral False = new BooleanLiteral(false);

		private readonly bool value;

		/// <summary>
		/// Represents a bool literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		public BooleanLiteral(bool Value)
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
		public Type Type => typeof(bool);

		/// <summary>
		/// Type name
		/// </summary>
		public string StringType => "http://www.w3.org/2001/XMLSchema#boolean";

		/// <summary>
		/// String representation of value.
		/// </summary>
		public string StringValue => CommonTypes.Encode(this.value);
	}
}
