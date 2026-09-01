using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Networking.XMPP.Contracts;

namespace Waher.Mcp.Identity.Responses
{
	/// <summary>
	/// Information about a peer review provider.
	/// </summary>
	public class PeerReviewProvider
	{
		/// <summary>
		/// Information about a peer review provider.
		/// </summary>
		/// <param name="Provider">Peer review provider.</param>
		public PeerReviewProvider(ServiceProviderWithLegalId Provider)
		{
			this.Id = Provider.Id;
			this.Type = Provider.Type;
			this.Name = Provider.Name;
			this.IconUrl = Provider.IconUrl;
			this.IconWidth = Provider.IconWidth;
			this.IconHeight = Provider.IconHeight;
			this.LegalId = Provider.LegalId;
			this.External = Provider.External;
		}

		/// <summary>
		/// ID of service provider.
		/// </summary>
		[McpStringParameter("Id", "Identifier of the particular service provider.")]
		public string Id { get; }

		/// <summary>
		/// Type of service provider.
		/// </summary>
		[McpStringParameter("Type", "Type name of service provider.")]
		public string Type { get; }

		/// <summary>
		/// Displayable name of service provider.
		/// </summary>
		[McpStringParameter("Name", "Displayable name of the service provider.")]
		public string Name { get; }

		/// <summary>
		/// Optional URL to icon of service provider.
		/// </summary>
		[McpStringParameter("IconUrl", "Icon for the service provider.")]
		public string IconUrl { get; }

		/// <summary>
		/// Width of icon, if available.
		/// </summary>
		[McpStringParameter("IconWidth", "Width of icon, in pixels.")]
		public int IconWidth { get; }

		/// <summary>
		/// Height of icon, if available.
		/// </summary>
		[McpStringParameter("IconHeight", "Height of icon, in pixels.")]
		public int IconHeight { get; }

		/// <summary>
		/// Legal identity
		/// </summary>
		[McpStringParameter("LegalId", "Identifier of Legal Identity of which peer reviews can be petitioned.")]
		public string LegalId { get; }

		/// <summary>
		/// If legal identity is external (true) or belongs to the server (false).
		/// </summary>
		[McpParameter("External", "If legal identity is external (true) or belongs to the server (false). Internal providers (false) must be selected before being petitioned.")]
		public bool External { get; }
	}
}
