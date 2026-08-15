using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Script;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Abstract base class for MCP-parameters that have a range of valid values.
	/// </summary>
	public abstract class McpRangeParameterAttribute : McpParameterAttribute
	{
		/// <summary>
		/// Abstract base class for MCP-parameters that have a range of valid values.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpRangeParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
			this.MinValue = null;
			this.MaxValue = null;
		}

		/// <summary>
		/// Abstract base class for MCP-parameters that have a range of valid values.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MinValue">Minimum value of integer, using the correct integer type.</param>
		/// <param name="MaxValue">Maximum value of integer, using the correct integer type.</param>
		public McpRangeParameterAttribute(string? Title, string? Description,
			object? MinValue, object? MaxValue)
			: base(Title, Description)
		{
			this.MinValue = MinValue;
			this.MaxValue = MaxValue;
		}

		/// <summary>
		/// Minimum value of the range.
		/// </summary>
		public object? MinValue { get; }

		/// <summary>
		/// Maximum value of the range.
		/// </summary>
		public object? MaxValue { get; }

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public override void Annotate(Dictionary<string, object?> Schema)
		{
			base.Annotate(Schema);

			if (!(this.MinValue is null))
				Schema["minimum"] = this.MinValue;

			if (!(this.MaxValue is null))
				Schema["maximum"] = this.MaxValue;
		}

		/// <summary>
		/// Annotated description of parameter.
		/// </summary>
		public override string AnnotatedDescription
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(base.AnnotatedDescription);

				if (this.MinValue is null)
					sb.Append(" \\(∞");
				else
				{
					sb.Append(" \\[`");
					sb.Append(Expression.ToExpressionString(this.MinValue));
					sb.Append('`');
				}

				sb.Append(',');

				if (this.MaxValue is null)
					sb.Append("∞\\)");
				else
				{
					sb.Append('`');
					sb.Append(Expression.ToExpressionString(this.MaxValue));
					sb.Append("`\\]");
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// Gets HTML input attributes for the parameter, if any.
		/// </summary>
		/// <param name="Attributes">Set of attributes.</param>
		public override void GetHtmlInputAttributes(Dictionary<string, string> Attributes)
		{
			if (!(this.MinValue is null))
				Attributes["min"] = this.GetHtmlAttributeValue(this.MinValue);

			if (!(this.MaxValue is null))
				Attributes["max"] = this.GetHtmlAttributeValue(this.MaxValue);
		}

		/// <summary>
		/// Checks if a value is valid for the parameter.
		/// </summary>
		/// <param name="Value">Value to check.</param>
		/// <returns>If the value is valid according to validation rules for the parameter.</returns>
		public override async Task<bool> IsValid(object Value)
		{
			if (Value is null)
				return false;

			if (!await base.IsValid(Value))
				return false;

			if (!(this.MinValue is null) && this.MinValue is IComparable MinComparable)
			{
				if (MinComparable.CompareTo(Value) > 0)
					return false;
			}

			if (!(this.MaxValue is null) && this.MaxValue is IComparable MaxComparable)
			{
				if (MaxComparable.CompareTo(Value) < 0)
					return false;
			}

			return true;
		}
	}
}
