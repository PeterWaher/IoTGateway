using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about an integer-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
	public class McpIntegerParameterAttribute : McpParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about an integer-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpIntegerParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
			this.MinValue = null;
			this.MaxValue = null;
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
			: base(Title, Description)
		{
			if (!IsIntegerType(MinValue))
				throw new ArgumentException("MinValue is not an integer type.", nameof(MinValue));

			if (!IsIntegerType(MaxValue))
				throw new ArgumentException("MaxValue is not an integer type.", nameof(MaxValue));

			this.MinValue = MinValue;
			this.MaxValue = MaxValue;
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
		/// Minimum value of integer.
		/// </summary>
		public object? MinValue { get; private set; }

		/// <summary>
		/// Maximum value of integer.
		/// </summary>
		public object? MaxValue { get; private set; }

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public override void Annotate(Dictionary<string, object?> Schema)
		{
			base.Annotate(Schema);

			Schema["type"] = "integer";

			if (!(this.MinValue is null))
				Schema["minimum"] = this.MinValue;

			if (!(this.MaxValue is null))
				Schema["maximum"] = this.MaxValue;
		}
	}
}
