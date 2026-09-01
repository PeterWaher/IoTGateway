using Waher.Mcp.Xmpp.Responses;
using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Identity.Responses
{
	/// <summary>
	/// Petition request response.
	/// </summary>
	public class PetitionIdResponse : GenericResponse
	{
		/// <summary>
		/// Petition request response.
		/// </summary>
		/// <param name="ErrorMessage">Error message.</param>
		public PetitionIdResponse(string ErrorMessage)
			: base(false, ErrorMessage)
		{
		}

		/// <summary>
		/// Petition request response.
		/// </summary>
		/// <param name="PetitionId">Petition identifier.</param>
		/// <param name="Message">Success message.</param>
		public PetitionIdResponse(string PetitionId, string Message)
			: base(true, Message)
		{
			this.PetitionId = PetitionId;
		}

		/// <summary>
		/// Peer review Providers.
		/// </summary>
		[McpStringParameter("PetitionId", "Petition identifier.")]
		public string? PetitionId { get; }
	}
}
