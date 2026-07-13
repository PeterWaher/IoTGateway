using System;
using System.Collections.Generic;
using Waher.Persistence.Attributes;

namespace Waher.Networking.HTTP.OAuth.Clients
{
	/// <summary>
	/// Contains information about an OAuth client, as defined in RFC 7591.
	/// </summary>
	[CollectionName("OAuthClients")]
	[TypeName(TypeNameSerialization.None)]
	[Index("ClientId")]
	public class OAuthClientInformation
	{
		/// <summary>
		/// Contains information about an OAuth client, as defined in RFC 7591.
		/// </summary>
		public OAuthClientInformation()
		{
		}

		/// <summary>
		/// Object ID of the client information object in the database.
		/// </summary>
		[ObjectId]
		public string? ObjectId { get; set; } = null;

		/// <summary>
		/// OAuth 2.0 client identifier string.
		/// </summary>
		public string? ClientId { get; set; } = null;

		/// <summary>
		/// When the client secret expires, if one is defined.
		/// </summary>
		public DateTime? ClientSecretExpiresAt { get; set; } = null;

		/// <summary>
		/// Access token required to update or delete the client information object.
		/// </summary>
		public string? AccessToken { get; set; } = null;

		/// <summary>
		/// When the client information was created.
		/// </summary>
		public DateTime Created { get; set; } = DateTime.MinValue;

		/// <summary>
		/// When the client information was last updated.
		/// </summary>
		public DateTime Updated { get; set; } = DateTime.MinValue;

		/// <summary>
		/// Remote endpoint of client making the registration request.
		/// </summary>
		public string? RemoteEndPoint { get; set; }

		/// <summary>
		/// Array of redirection URI strings for use in redirect-based flows
		/// such as the authorization code and implicit flows.
		/// </summary>
		public string[]? RedirectUris { get; set; }

		/// <summary>
		/// Array of OAuth 2.0 grant type strings that the client can use at
		/// the token endpoint.
		/// </summary>
		public string[]? GrantTypes { get; set; }

		/// <summary>
		/// Array of the OAuth 2.0 response type strings that the client can
		/// use at the authorization endpoint.
		/// </summary>
		public string[]? ResponseTypes { get; set; }

		/// <summary>
		/// String indicator of the requested authentication method for the
		/// token endpoint.
		/// </summary>
		public string? TokenEndpointAuthMethod { get; set; }

		/// <summary>
		/// Human-readable string name of the client to be presented to the
		/// end-user during authorization.
		/// </summary>
		public string? ClientName { get; set; }

		/// <summary>
		/// A unique identifier string (e.g., a Universally Unique Identifier
		/// (UUID)) assigned by the client developer or software publisher
		/// used by registration endpoints to identify the client software to
		/// be dynamically registered.
		/// </summary>
		public string? SoftwareId { get; set; }

		/// <summary>
		/// A version identifier string for the client software identified by
		/// "software_id".
		/// </summary>
		public string? SoftwareVersion { get; set; }

		/// <summary>
		/// URL string of a web page providing information about the client.
		/// </summary>
		public string? ClientUri { get; set; }

		/// <summary>
		/// URL string that references a logo for the client.
		/// </summary>
		public string? LogoUri { get; set; }

		/// <summary>
		/// URL string that points to a human-readable terms of service
		/// document for the client that describes a contractual relationship
		/// between the end-user and the client that the end-user accepts when
		/// authorizing the client.
		/// </summary>
		public string? TosUri { get; set; }

		/// <summary>
		/// URL string that points to a human-readable privacy policy document
		/// that describes how the deployment organization collects, uses,
		/// retains, and discloses personal data.
		/// </summary>
		public string? PolicyUri { get; set; }

		/// <summary>
		/// URL string referencing the client's JSON Web Key (JWK) Set
		/// [RFC7517] document, which contains the client's public keys.
		/// </summary>
		public string? JwksUri { get; set; }

		/// <summary>
		/// List of scope values (as described in Section 3.3 of OAuth 2.0 [RFC6749]) 
		/// that the client can use when requesting access tokens.
		/// </summary>
		public string[]? Scopes { get; set; }

		/// <summary>
		/// Array of strings representing ways to contact people responsible
		/// for this client, typically email addresses.
		/// </summary>
		public string[]? Contacts { get; set; }

		/// <summary>
		/// Client's JSON Web Key Set [RFC7517] document value, which contains
		/// the client's public keys.
		/// </summary>
		public Dictionary<string, object?>? Jwks { get; set; }

		/// <summary>
		/// Additional meta-data available in the request.
		/// </summary>
		public Dictionary<string, object?>? MetaData { get; set; }
	}
}
