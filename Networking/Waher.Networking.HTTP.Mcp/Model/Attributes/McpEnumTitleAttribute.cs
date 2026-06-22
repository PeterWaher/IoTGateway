using System;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides a title to an enumeration value.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = true)]
	public class McpEnumValueAttribute : Attribute
	{
		/// <summary>
		/// Provides a title to an enumeration value.
		/// </summary>
		/// <param name="Value">Enumeration value.</param>
		/// <param name="Title">Title of enumeration value.</param>
		public McpEnumValueAttribute(object Value, string? Title)
		{
			if (!(Value is Enum EnumValue))
				throw new ArgumentException("Value must be an enumeration value.", nameof(Value));

			this.Value = EnumValue;
			this.Title = Title;
		}

		/// <summary>
		/// Enumeration value
		/// </summary>
		public Enum Value { get; private set; }

		/// <summary>
		/// Title of enumeration value.
		/// </summary>
		public string? Title { get; private set; }
	}
}
