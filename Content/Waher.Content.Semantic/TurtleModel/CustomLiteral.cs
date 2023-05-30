using System.Text;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a custom literal.
	/// </summary>
	public class CustomLiteral : SemanticLiteral
	{
		private bool? multiLine;

		/// <summary>
		/// Represents a custom literal.
		/// </summary>
		/// <param name="Value">Literal value</param>
		/// <param name="Type">Data type.</param>
		public CustomLiteral(string Value, string Type)
			: base(Value, Value)
		{
			this.StringType = Type;
		}

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType { get; }

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType)
		{
			return new CustomLiteral(Value, DataType);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			if (!this.multiLine.HasValue)
				this.multiLine = this.StringValue.IndexOfAny(CommonTypes.CRLF) >= 0;

			StringBuilder sb = new StringBuilder();

			if (this.multiLine.Value)
			{
				sb.Append("\"\"\"");
				sb.Append(JSON.Encode(this.StringValue));
				sb.Append("\"\"\"");
			}
			else
			{
				sb.Append('"');
				sb.Append(JSON.Encode(this.StringValue));
				sb.Append('"');
			}

			sb.Append("^^\"");
			sb.Append(this.StringType);
			sb.Append('"');

			return sb.ToString();
		}
	}
}
