using System;
using Waher.Persistence.Attributes;

namespace Waher.Networking.HTTP.OAuth.Clients
{
	/// <summary>
	/// Contains information about a redirect URI using by an OAuth client.
	/// </summary>
	[CollectionName("OAuthRedirectUris")]
	[TypeName(TypeNameSerialization.None)]
	[Index("Uri")]
	[Index("ClientId")]
	public class OAuthRedirectUri
	{
		/// <summary>
		/// Contains information about a redirect URI using by an OAuth client.
		/// </summary>
		public OAuthRedirectUri()
		{
		}

		/// <summary>
		/// Object ID of the redirect URI.
		/// </summary>
		[ObjectId]
		public string? ObjectId { get; set; } = null;

		/// <summary>
		/// OAuth 2.0 client identifier string.
		/// </summary>
		public string ClientId { get; set; } = string.Empty;

		/// <summary>
		/// Redirection URI.
		/// </summary>
		public string Uri { get; set; } = string.Empty;
	}
}
