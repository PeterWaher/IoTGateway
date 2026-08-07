using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Xmpp.UserInput
{
	/// <summary>
	/// Class containing user input parameters for XMPP credentials.
	/// </summary>
	internal class PresenceSubscriptionRequest
	{
		/// <summary>
		/// If the presence subscription should be mutual, in both directions.
		/// </summary>
		[McpParameter("Follow Back", "If the presence subscription should be mutual, in both directions.")]
		public bool Mutual = false;
	}
}
