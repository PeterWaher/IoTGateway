using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Identity.UserInput
{
	/// <summary>
	/// Class containing a petition.
	/// </summary>
	internal class Petition
	{
		[McpParameter("Accept", "If you accept the petition, and the requested information should be returned.")]
		public bool? Accept;
	}
}
