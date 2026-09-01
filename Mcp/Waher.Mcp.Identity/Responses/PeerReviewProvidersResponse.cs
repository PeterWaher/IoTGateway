using Waher.Mcp.Xmpp.Responses;
using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Identity.Responses
{
	/// <summary>
	/// Peer review Providers response.
	/// </summary>
	public class PeerReviewProvidersResponse : GenericResponse
	{
		/// <summary>
		/// Peer review Providers response.
		/// </summary>
		/// <param name="ErrorMessage">Error message.</param>
		public PeerReviewProvidersResponse(string ErrorMessage)
			: base(false, ErrorMessage)
		{
		}

		/// <summary>
		/// Peer review Providers response.
		/// </summary>
		/// <param name="Providers">Service providers.</param>
		public PeerReviewProvidersResponse(PeerReviewProvider[] Providers)
			: base(true, "Peer review providers retrieved.")
		{
			this.Providers = Providers;
		}

		/// <summary>
		/// Peer review Providers.
		/// </summary>
		[McpStringParameter("Providers", "Peer review providers.")]
		public PeerReviewProvider[]? Providers { get; }
	}
}
