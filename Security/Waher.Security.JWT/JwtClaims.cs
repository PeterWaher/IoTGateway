using System;

namespace Waher.Security.JWT
{
	/// <summary>
	/// Static class containing predefined JWT claim names.
	/// 
	/// Source:
	/// https://www.iana.org/assignments/jwt/jwt.xhtml#claims
	/// </summary>
	public static class JwtClaims
	{
		/// <summary>
		/// Issuer of the JWT
		/// </summary>
		public const string Issuer = "iss";

		/// <summary>
		/// Subject of the JWT (the user)
		/// </summary>
		public const string Subject = "sub";

		/// <summary>
		/// Recipient for which the JWT is intended
		/// </summary>
		public const string Audience = "aud";

		/// <summary>
		/// Time after which the JWT expires
		/// </summary>
		public const string ExpirationTime = "exp";

		/// <summary>
		/// Time before which the JWT must not be accepted for processing
		/// </summary>
		public const string NotBeforeTime = "nbf";

		/// <summary>
		/// Time at which the JWT was issued; can be used to determine age of the JWT
		/// </summary>
		public const string IssueTime = "iat";

		/// <summary>
		/// Unique identifier; can be used to prevent the JWT from being replayed (allows a token to be used only once)
		/// </summary>
		public const string JwtId = "jti";

		/// <summary>
		/// Actor
		/// </summary>
		public const string Actor = "act";

		/// <summary>
		/// Scope Values
		/// </summary>
		public const string Scope = "scope";

		/// <summary>
		/// Client identifier
		/// </summary>
		public const string ClientId = "client_id";
	}
}
