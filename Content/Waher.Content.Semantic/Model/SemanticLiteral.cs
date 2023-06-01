using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Abstract base class for semantic literal values.
	/// </summary>
	public abstract class SemanticLiteral : ISemanticLiteral
	{
		/// <summary>
		/// Abstract base class for semantic literal values.
		/// </summary>
		public SemanticLiteral()
		{
		}

		/// <summary>
		/// Abstract base class for semantic literal values.
		/// </summary>
		/// <param name="Value">Parsed Value</param>
		/// <param name="StringValue">String Value</param>
		public SemanticLiteral(object Value, string StringValue)
		{
			this.Value = Value;
			this.StringValue = StringValue;
		}

 		/// <summary>
		/// Parsed value.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// Type name (or null if literal value is a string)
		/// </summary>
		public abstract string StringType { get; }

		/// <summary>
		/// String representation of value.
		/// </summary>
		public string StringValue { get; }

		/// <summary>
		/// How well the type supports a given data type.
		/// </summary>
		/// <param name="DataType">Data type.</param>
		/// <returns>Support grade.</returns>
		public virtual Grade Supports(string DataType)
		{
			return DataType == this.StringType ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <returns>Parsed literal.</returns>
		public abstract ISemanticLiteral Parse(string Value, string DataType);

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append('"');
			sb.Append(JSON.Encode(this.StringValue));
			sb.Append("\"^^<");
			sb.Append(this.StringType);
			sb.Append('>');

			return sb.ToString();
		}

		/// <inheritdoc/>
		public override abstract bool Equals(object obj);

		/// <inheritdoc/>
		public override abstract int GetHashCode();
	}
}
