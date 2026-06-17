using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Client
{
	/// <summary>
	/// Describes the MCP implementation.
	/// </summary>
	public class Implementation
	{
		/// <summary>
		/// Version of the implementation, if available.
		/// </summary>
		public string? Version { get; internal set; }

		/// <summary>
		/// An optional human-readable description of what this implementation does.
		/// 
		/// This can be used by clients or servers to provide context about their purpose
		/// and capabilities.For example, a server might describe the types of resources
		/// or tools it provides, while a client might describe its intended use case.
		/// </summary>
		public string? Description { get; internal set; }

		/// <summary>
		/// An optional URL of the website for this implementation.
		/// </summary>
		public Uri? WebsiteUrl { get; internal set; }

		/// <summary>
		/// Base metadata about the implementation, if available.
		/// </summary>
		public BaseMetadata? Metadata { get; internal set; }

		/// <summary>
		/// Available icons for this implementation, if any.
		/// </summary>
		public Icons? Icons { get; internal set; }

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic,
			out Implementation Typed)
		{
			Implementation Result = new Implementation();

			if (Generic.TryGetValue("version", out object? Obj))
				Result.Version = Obj as string;

			if (Generic.TryGetValue("description", out Obj))
				Result.Description = Obj as string;

			if (Generic.TryGetValue("websiteUrl", out Obj) &&
				Obj is string WebsiteUrl &&
				Uri.TryCreate(WebsiteUrl, UriKind.Absolute, out Uri WebsiteUrlUri))
			{
				Result.WebsiteUrl = WebsiteUrlUri;
			}

			if (BaseMetadata.TryParse(Generic, out BaseMetadata? Metadata))
				Result.Metadata = Metadata;

			if (Icons.TryParse(Generic, out Icons? IconsParsed))
				Result.Icons = IconsParsed;

			Typed = Result;
			return true;
		}
	}
}
