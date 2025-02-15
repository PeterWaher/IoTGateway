using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Script;
using Waher.Script.Exceptions;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Abstract base class for report attributes.
	/// </summary>
	/// <typeparam name="T">Type of attribute value.</typeparam>
	public abstract class ReportAttribute<T>
	{
		private readonly string definition;
		private T constant;
		private Expression expression;
		private readonly bool isExpression;
		private bool first = true;

		/// <summary>
		/// Abstract base class for report attributes.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportAttribute(XmlElement Xml, string AttributeName)
		{
			if (string.IsNullOrEmpty(AttributeName))
				this.definition = Xml.InnerText;
			else
				this.definition = XML.Attribute(Xml, AttributeName);

			this.isExpression = this.definition.StartsWith('{') && this.definition.EndsWith('}');
		}

		/// <summary>
		/// Attribute definition
		/// </summary>
		public string Definition => this.definition;

		/// <summary>
		/// If the attribute value is a constant.
		/// </summary>
		public bool IsConstant => !this.isExpression;

		/// <summary>
		/// If the attribute value is an expression.
		/// </summary>
		public bool IsExpression => this.isExpression;

		/// <summary>
		/// If the definition is empty.
		/// </summary>
		public bool IsEmpty => string.IsNullOrEmpty(this.definition);

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">String representation</param>
		/// <returns>Parsed value.</returns>
		public abstract T ParseValue(string s);

		/// <summary>
		/// Evaluates the attribute
		/// </summary>
		/// <param name="Variables">Available variables.</param>
		/// <returns>Attribute value.</returns>
		public async Task<T> Evaluate(Variables Variables)
		{
			if (this.first)
			{
				if (this.isExpression)
					this.expression = new Expression(this.definition[1..^1]);
				else
					this.constant = this.ParseValue(this.definition);

				this.first = false;
			}

			if (this.isExpression)
			{
				object Result = await this.expression.EvaluateAsync(Variables);

				if (Result is T TypedResult)
					return TypedResult;
				else
				{
					StringBuilder sb = new StringBuilder();

					sb.Append("Expected a value of type ");
					sb.Append(typeof(T).FullName);
					sb.Append(" but got a value of type ");
					sb.Append(Result.GetType().FullName);
					sb.Append('.');

					throw new ScriptRuntimeException(sb.ToString(), this.expression.Root);
				}
			}
			else
				return this.constant;
		}
	}
}
