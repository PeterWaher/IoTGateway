using Waher.Mcp.Xmpp.Responses;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Networking.XMPP.Contracts.EventArguments;

namespace Waher.Mcp.Identity.Responses
{
	/// <summary>
	/// Identity application attributes response.
	/// </summary>
	public class IdentityApplicationAttributesResponse : GenericResponse
	{
		/// <summary>
		/// Identity application attributes response.
		/// </summary>
		/// <param name="ErrorMessage">Error message.</param>
		public IdentityApplicationAttributesResponse(string ErrorMessage)
			: base(false, ErrorMessage)
		{
			this.PeerReview = null;
			this.NrReviewers = null;
			this.NrPhotos = null;
			this.Iso3166 = null;
			this.RequiredProperties = null;
		}

		/// <summary>
		/// Identity application attributes response.
		/// </summary>
		/// <param name="e">Identity application attributes.</param>
		public IdentityApplicationAttributesResponse(IdApplicationAttributesEventArgs e)
			: base(true, "Attributes retrieved.")
		{
			this.PeerReview = e.PeerReview;
			this.NrReviewers = e.NrReviewers;
			this.NrPhotos = e.NrPhotos;
			this.Iso3166 = e.Iso3166;
			this.RequiredProperties = e.RequiredProperties;
		}

		/// <summary>
		/// If peer-review is allowed as a mechanism to approve ID applications.
		/// </summary>
		[McpParameter("Peer Review", "If peer-review is allowed as a mechanism to approve ID applications.")]
		public bool? PeerReview;

		/// <summary>
		/// Number of peer reviewers required to get an ID approved using peer review.
		/// </summary>
		[McpIntegerParameter("Nr Reviewers", "Number of peer reviewers required to get an ID approved using peer review.", 0, null)]
		public int? NrReviewers;

		/// <summary>
		/// Number of photos required in a peer-review.
		/// </summary>
		[McpIntegerParameter("Nr Photos", "Number of photos required in a peer-review.", 0, null)]
		public int? NrPhotos;

		/// <summary>
		/// If ISO 3166 country codes are mandated in peer-review.
		/// </summary>
		[McpParameter("ISO 3166", "If ISO 3166 country codes are mandated in peer-review.")]
		public bool? Iso3166;

		/// <summary>
		/// Required properties in an ID application for peer-review.
		/// </summary>
		[McpParameter("Required Properties", "Required properties in an ID application for peer-review.")]
		public string[]? RequiredProperties;
	}
}
