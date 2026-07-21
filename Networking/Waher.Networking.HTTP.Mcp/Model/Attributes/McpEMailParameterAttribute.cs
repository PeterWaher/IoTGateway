using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a e-mail-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue |
		AttributeTargets.Property | AttributeTargets.Field, 
		Inherited = true, AllowMultiple = false)]
	public class McpEMailParameterAttribute : McpStringParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about a e-mail-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpEMailParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		public McpEMailParameterAttribute(string? Title, string? Description,
			int MaxLength)
			: base(Title, Description, MaxLength)
		{
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MinLength">Minimum length of string.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		public McpEMailParameterAttribute(string? Title, string? Description,
			int MinLength, int MaxLength)
			: base(Title, Description, MinLength, MaxLength)
		{
		}

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public override void Annotate(Dictionary<string, object?> Schema)
		{
			base.Annotate(Schema);

			Schema["format"] = "email";
		}
	}
}
