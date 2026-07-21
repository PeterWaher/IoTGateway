using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue |
		AttributeTargets.Property | AttributeTargets.Field, 
		Inherited = true, AllowMultiple = false)]
	public class McpParameterAttribute : Attribute
	{
		/// <summary>
		/// Provides meta-data about a parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpParameterAttribute(string? Title, string? Description)
		{
			this.Title = Title;
			this.Description = Description;
		}

		/// <summary>
		/// Title of parameter.
		/// </summary>
		public string? Title { get; }

		/// <summary>
		/// Description of parameter.
		/// </summary>
		public string? Description { get; }

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public virtual void Annotate(Dictionary<string, object?> Schema)
		{
			if (!string.IsNullOrEmpty(this.Title))
				Schema["title"] = this.Title;

			if (!string.IsNullOrEmpty(this.Description))
				Schema["description"] = this.Description;
		}
	}
}
