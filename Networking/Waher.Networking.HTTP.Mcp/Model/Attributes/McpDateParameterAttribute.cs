using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a date-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
	public class McpDateParameterAttribute : McpStringParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about a date-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpDateParameterAttribute(string? Title, string? Description)
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

			Schema["format"] = "date";
		}
	}
}
