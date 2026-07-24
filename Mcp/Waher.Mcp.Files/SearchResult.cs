using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Files
{
	/// <summary>
	/// Contains a search result of files in account-specific storage.
	/// </summary>
	public class SearchResult
	{
		/// <summary>
		/// Contains a search result of files in account-specific storage.
		/// </summary>
		/// <param name="NrMatches">Number of matches found.</param>
		/// <param name="NrFilesTotal">Total number of files in account-specific file storage.</param>
		/// <param name="Pattern">Search pattern used.</param>
		/// <param name="FileNames">File names matching the search pattern.</param>
		/// <param name="ResourceUris">Resource URIs matching the search pattern.</param>
		public SearchResult(int NrMatches, int NrFilesTotal, string Pattern, 
			string[] FileNames, string[] ResourceUris)
		{
			this.NrMatches = NrMatches;
			this.NrFilesTotal = NrFilesTotal;
			this.Pattern = Pattern;
			this.FileNames = FileNames;
			this.ResourceUris = ResourceUris;
		}

		/// <summary>
		/// Number of matches found.
		/// </summary>
		[McpIntegerParameter("NrMatches", "Number of matches found.", 0, null)]
		public int NrMatches { get; }

		/// <summary>
		/// Total number of files in account-specific file storage.
		/// </summary>
		[McpIntegerParameter("NrFilesTotal", "Total number of files in account-specific file storage.", 0, null)]
		public int NrFilesTotal { get; }

		/// <summary>
		/// Search pattern used.
		/// </summary>
		[McpStringParameter("Pattern", "Search pattern used.")]
		public string Pattern { get; }

		/// <summary>
		/// File names matching the search pattern.
		/// </summary>
		[McpParameter("FileNames", "File names matching the search pattern.")]
		public string[] FileNames { get; }

		/// <summary>
		/// Resource URIs matching the search pattern.
		/// </summary>
		[McpParameter("ResourceUris", "Resource URIs matching the search pattern.")]
		public string[] ResourceUris { get; }
	}
}
