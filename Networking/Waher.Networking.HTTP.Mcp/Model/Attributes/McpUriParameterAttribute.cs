using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a URI-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue |
		AttributeTargets.Property | AttributeTargets.Field, 
		Inherited = true, AllowMultiple = false)]
	public class McpUriParameterAttribute : McpStringParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about a URI-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpUriParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
			this.ContentType = null;
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		public McpUriParameterAttribute(string? Title, string? Description,
			int MaxLength)
			: base(Title, Description, MaxLength)
		{
			this.ContentType = null;
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MinLength">Minimum length of string.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		public McpUriParameterAttribute(string? Title, string? Description,
			int MinLength, int MaxLength)
			: base(Title, Description, MinLength, MaxLength)
		{
			this.ContentType = null;
		}

		/// <summary>
		/// Provides meta-data about a URI-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpUriParameterAttribute(string? Title, string? Description, string ContentType)
			: base(Title, Description)
		{
			this.ContentType = ContentType;
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		public McpUriParameterAttribute(string? Title, string? Description,
			int MaxLength, string ContentType)
			: base(Title, Description, MaxLength)
		{
			this.ContentType = ContentType;
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MinLength">Minimum length of string.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		public McpUriParameterAttribute(string? Title, string? Description,
			int MinLength, int MaxLength, string ContentType)
			: base(Title, Description, MinLength, MaxLength)
		{
			this.ContentType = ContentType;
		}

		/// <summary>
		/// Content-Type of the content the URI points to.
		/// </summary>
		public string? ContentType { get; set; }

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public override void Annotate(Dictionary<string, object?> Schema)
		{
			base.Annotate(Schema);

			Schema["format"] = "uri";
			
			if (!string.IsNullOrEmpty(this.ContentType))
				Schema["mimeType"] = this.ContentType;
		}
	}
}
