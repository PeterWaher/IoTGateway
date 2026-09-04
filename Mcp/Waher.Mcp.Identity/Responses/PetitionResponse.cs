using System.Collections.Generic;
using Waher.Mcp.Xmpp.Responses;
using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Identity.Responses
{
	/// <summary>
	/// Petition request response.
	/// </summary>
	public class PetitionResponse : GenericResponse
	{
		/// <summary>
		/// Petition request response.
		/// </summary>
		/// <param name="ErrorMessage">Error message.</param>
		public PetitionResponse(string ErrorMessage)
			: base(false, ErrorMessage)
		{
		}

		/// <summary>
		/// Petition request response.
		/// </summary>
		/// <param name="PetitionId">Petition identifier.</param>
		/// <param name="Message">Success message.</param>
		public PetitionResponse(string PetitionId, string Message)
			: base(true, Message)
		{
			this.PetitionId = PetitionId;
		}

		/// <summary>
		/// Petition request response.
		/// </summary>
		/// <param name="QuickResponse">Requested object available, either in cache or via 
		/// earlier granted authorization.</param>
		/// <param name="Message">Success message.</param>
		public PetitionResponse(Dictionary<string, object?>? QuickResponse, string Message)
			: base(true, Message)
		{
			this.QuickResponse = QuickResponse;
		}

		/// <summary>
		/// Peer review Providers.
		/// </summary>
		[McpStringParameter("PetitionId", "Petition identifier.")]
		public string? PetitionId { get; }

		/// <summary>
		/// Quick response, if available.
		/// </summary>
		[McpParameter("QuickResponse", "Requested object available, either in cache or via earlier granted authorization.")]
		public Dictionary<string, object?>? QuickResponse { get; }
	}
}
