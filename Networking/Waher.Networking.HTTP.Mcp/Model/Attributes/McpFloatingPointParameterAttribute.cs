using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a floating-point-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
	public class McpFloatingPointParameterAttribute : McpParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about a floating-point-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpFloatingPointParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
			this.MinValue = null;
			this.MaxValue = null;
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
			: base(Title, Description)
		{
			if (!IsFloatingPointType(MinValue))
				throw new ArgumentException("MinValue is not a float-point type.", nameof(MinValue));

			if (!IsFloatingPointType(MaxValue))
				throw new ArgumentException("MaxValue is not a float-point type.", nameof(MaxValue));

			this.MinValue = MinValue;
			this.MaxValue = MaxValue;
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
		/// Minimum value of floating-point value.
		/// </summary>
		public object? MinValue { get; private set; }

		/// <summary>
		/// Minimum value of floating-point value.
		/// </summary>
		public object? MaxValue { get; private set; }

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public override void Annotate(Dictionary<string, object?> Schema)
		{
			base.Annotate(Schema);

			Schema["type"] = "number";

			if (!(this.MinValue is null))
				Schema["minimum"] = this.MinValue;

			if (!(this.MaxValue is null))
				Schema["maximum"] = this.MaxValue;
		}
	}
}
