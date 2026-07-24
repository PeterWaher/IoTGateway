using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a floating-point-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue |
		AttributeTargets.Property | AttributeTargets.Field,
		Inherited = true, AllowMultiple = false)]
	public class McpFloatingPointParameterAttribute : McpRangeParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about a floating-point-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpFloatingPointParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
		}

		/// <summary>
		/// Provides meta-data about a floating-point-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MinValue">Minimum value of integer, using the correct integer type.</param>
		/// <param name="MaxValue">Maximum value of integer, using the correct integer type.</param>
		public McpFloatingPointParameterAttribute(string? Title, string? Description,
			object? MinValue, object? MaxValue)
			: base(Title, Description, MinValue, MaxValue)
		{
			if (!IsFloatingPointType(MinValue))
				throw new ArgumentException("MinValue is not a float-point type.", nameof(MinValue));

			if (!IsFloatingPointType(MaxValue))
				throw new ArgumentException("MaxValue is not a float-point type.", nameof(MaxValue));
		}

		private static bool IsFloatingPointType(object? Value)
		{
			if (Value is null)
				return true;

			switch (Type.GetTypeCode(Value.GetType()))
			{
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
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

			Schema["type"] = "number";
		}
	}
}
