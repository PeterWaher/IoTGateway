using System;

namespace Waher.Networking.HTTP.OAuth.Interfaces
{
	/// <summary>
	/// Dynamic client registration, as defined in RFC 7591.
	/// </summary>
	public interface IRegistration
	{
		/// <summary>
		/// OAuth 2.0 client identifier string.
		/// </summary>
		string ClientId { get; }

		/// <summary>
		/// OAuth 2.0 client secret string.
		/// </summary>
		string ClientSecret { get; }

		/// <summary>
		/// Time at which the client secret will expire
		/// </summary>
		DateTime? ClientSecretExpiresAt { get; }
	}
}
