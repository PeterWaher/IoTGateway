using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.OAuth.Interfaces
{
	/// <summary>
	/// Dynamic client registration request, as defined in RFC 7591.
	/// </summary>
	public interface IRegistrationRequest
	{
		/// <summary>
		/// Remote endpoint of client making the registration request.
		/// </summary>
		string RemoteEndPoint { get; }

		/// <summary>
		/// If the client is a public client.
		/// </summary>
		bool PublicClient { get; }

		/// <summary>
		/// If the client is a confidential client.
		/// </summary>
		bool ConfidentialClient { get; }

		/// <summary>
		/// Array of redirection URI strings for use in redirect-based flows
		/// such as the authorization code and implicit flows.
		/// </summary>
		string[]? RedirectUris { get; }

		/// <summary>
		/// Array of OAuth 2.0 grant type strings that the client can use at
		/// the token endpoint.
		/// </summary>
		string[]? GrantTypes { get; }

		/// <summary>
		/// Array of the OAuth 2.0 response type strings that the client can
		/// use at the authorization endpoint.
		/// </summary>
		string[]? ResponseTypes { get; }

		/// <summary>
		/// String indicator of the requested authentication method for the
		/// token endpoint.
		/// </summary>
		string? TokenEndpointAuthMethod { get; }

		/// <summary>
		/// Human-readable string name of the client to be presented to the
		/// end-user during authorization.
		/// </summary>
		string? ClientName { get; }

		/// <summary>
		/// A unique identifier string (e.g., a Universally Unique Identifier
		/// (UUID)) assigned by the client developer or software publisher
		/// used by registration endpoints to identify the client software to
		/// be dynamically registered.
		/// </summary>
		string? SoftwareId { get; }

		/// <summary>
		/// A version identifier string for the client software identified by
		/// "software_id".
		/// </summary>
		string? SoftwareVersion { get; }

		/// <summary>
		/// URL string of a web page providing information about the client.
		/// </summary>
		Uri? ClientUri { get; }

		/// <summary>
		/// URL string that references a logo for the client.
		/// </summary>
		Uri? LogoUri { get; }

		/// <summary>
		/// URL string that points to a human-readable terms of service
		/// document for the client that describes a contractual relationship
		/// between the end-user and the client that the end-user accepts when
		/// authorizing the client.
		/// </summary>
		Uri? TosUri { get; }

		/// <summary>
		/// URL string that points to a human-readable privacy policy document
		/// that describes how the deployment organization collects, uses,
		/// retains, and discloses personal data.
		/// </summary>
		Uri? PolicyUri { get; }

		/// <summary>
		/// URL string referencing the client's JSON Web Key (JWK) Set
		/// [RFC7517] document, which contains the client's public keys.
		/// </summary>
		Uri? JwksUri { get; }

		/// <summary>
		/// List of scope values (as described in Section 3.3 of OAuth 2.0 [RFC6749]) 
		/// that the client can use when requesting access tokens.
		/// </summary>
		string[]? Scopes { get; }

		/// <summary>
		/// Array of strings representing ways to contact people responsible
		/// for this client, typically email addresses.
		/// </summary>
		string[]? Contacts { get; }

		/// <summary>
		/// Client's JSON Web Key Set [RFC7517] document value, which contains
		/// the client's public keys.
		/// </summary>
		Dictionary<string, object?>? Jwks { get; }

		/// <summary>
		/// Additional meta-data available in the request.
		/// </summary>
		Dictionary<string, object?>? MetaData { get; }

		/// <summary>
		/// OAuth 2.0 client identifier string.
		/// </summary>
		string? ClientId { get; }

		/// <summary>
		/// OAuth 2.0 client secret string.
		/// </summary>
		string? ClientSecret { get; }

	}
}
