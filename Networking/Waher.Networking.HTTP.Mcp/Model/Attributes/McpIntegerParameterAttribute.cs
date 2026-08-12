using System;
using System.Collections.Generic;
using Waher.Script;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about an integer-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | 
		AttributeTargets.Property | AttributeTargets.Field, 
		Inherited = true, AllowMultiple = false)]
	public class McpIntegerParameterAttribute : McpRangeParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about an integer-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpIntegerParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
		}

		/// <summary>
		/// Provides meta-data about a integer-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MinValue">Minimum value of integer, using the correct integer type.</param>
		/// <param name="MaxValue">Maximum value of integer, using the correct integer type.</param>
		public McpIntegerParameterAttribute(string? Title, string? Description,
			object? MinValue, object? MaxValue)
			: base(Title, Description, MinValue, MaxValue)
		{
			if (!IsIntegerType(MinValue))
				throw new ArgumentException("MinValue is not an integer type.", nameof(MinValue));

			if (!IsIntegerType(MaxValue))
				throw new ArgumentException("MaxValue is not an integer type.", nameof(MaxValue));
		}

		private static bool IsIntegerType(object? Value)
		{
			if (Value is null)
				return true;

			switch (Type.GetTypeCode(Value.GetType()))
			{
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public override void Annotate(Dictionary<string, object?> Schema)
		{
			base.Annotate(Schema);

			Schema["type"] = "integer";
		}

		/// <summary>
		/// Gets HTML input attributes for the parameter, if any.
		/// </summary>
		/// <param name="Attributes">Set of attributes.</param>
		public override void GetHtmlInputAttributes(Dictionary<string, string> Attributes)
		{
			base.GetHtmlInputAttributes(Attributes);

			if (this.MinValue is null || this.MaxValue is null)
				Attributes["type"] = "number";
			else
				Attributes["type"] = "range";

			Attributes["step"] = "1";
		}

		/// <summary>
		/// Gets the HTML attribute value for a parameter, if any.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>HTML attribute value</returns>
		public override string GetHtmlAttributeValue(object Value)
		{
			return Expression.ToExpressionString(Value);
		}

		/// <summary>
		/// Checks if a value is valid for the parameter.
		/// </summary>
		/// <param name="Value">Value to check.</param>
		/// <returns>If the value is valid according to validation rules for the parameter.</returns>
		public override bool IsValid(object Value)
		{
			return !(Value is null) && IsIntegerType(Value);
		}
	}
}
