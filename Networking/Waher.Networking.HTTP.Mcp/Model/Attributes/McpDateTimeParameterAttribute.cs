using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a date and time-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue |
		AttributeTargets.Property | AttributeTargets.Field, 
		Inherited = true, AllowMultiple = false)]
	public class McpDateTimeParameterAttribute : McpStringParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about a date and time-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpDateTimeParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
		}

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public override void Annotate(Dictionary<string, object?> Schema)
		{
			base.Annotate(Schema);

			Schema["format"] = "date-time";
		}
	}
}
