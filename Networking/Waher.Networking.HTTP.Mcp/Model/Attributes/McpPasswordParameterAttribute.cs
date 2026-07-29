using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a masked string-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue |
		AttributeTargets.Property | AttributeTargets.Field, 
		Inherited = true, AllowMultiple = false)]
	public class McpPasswordParameterAttribute : McpStringParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about a masked string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpPasswordParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
		}

		/// <summary>
		/// Provides meta-data about a masked string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		public McpPasswordParameterAttribute(string? Title, string? Description,
			int MaxLength)
			: base(Title, Description, MaxLength)
		{
		}

		/// <summary>
		/// Provides meta-data about a masked string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MinLength">Minimum length of string.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		public McpPasswordParameterAttribute(string? Title, string? Description,
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
			throw new ServiceUnavailableException("Passwords not permitted in schema. User input via URL is required.");
		}

		/// <summary>
		/// Gets HTML input attributes for the parameter, if any.
		/// </summary>
		/// <param name="Attributes">Set of attributes.</param>
		public override void GetHtmlInputAttributes(Dictionary<string, string> Attributes)
		{
			base.GetHtmlInputAttributes(Attributes);
			Attributes["type"] = "password";
		}
	}
}
