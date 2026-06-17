using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model
{
	/// <summary>
	/// Base interface for metadata with name (identifier) and title (display name) 
	/// properties.
	/// </summary>
	public class BaseMetadata
	{
		/// <summary>
		/// Intended for programmatic or logical use, but used as a display name in past 
		/// specs or fallback (if title isn't present).
		/// </summary>
		public string? Name { get; internal set; }

		/// <summary>
		/// Intended for UI and end-user contexts — optimized to be human-readable and 
		/// easily understood, even by those unfamiliar with domain-specific terminology.
		/// 
		/// If not provided, the name should be used for display (except for Tool,
		/// where `annotations.title` should be given precedence over using `name`,
		/// if present).
		/// </summary>
		public string? Title { get; internal set; }

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic,
			out BaseMetadata Typed)
		{
			BaseMetadata Result = new BaseMetadata();

			if (Generic.TryGetValue("name", out object? Obj))
				Result.Name = Obj as string;

			if (Generic.TryGetValue("title", out Obj))
				Result.Title = Obj as string;

			Typed = Result;
			return true;
		}
	}
}
