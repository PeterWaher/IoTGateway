using System;

namespace Waher.Security.JWT
{
	/// <summary>
	/// Static class containing predefined OpenID JWT claim names.
	/// 
	/// Source:
	/// https://www.iana.org/assignments/jwt/jwt.xhtml#claims
	/// </summary>
	public static class OpenIdClaims
	{
		/// <summary>
		/// Full Name
		/// </summary>
		public const string FullName = "name";

		/// <summary>
		/// Given name(s) or first name(s)		
		/// </summary>
		public const string GivenName = "given_name";

		/// <summary>
		/// Surname(s) or last name(s)		
		/// </summary>
		public const string Surname = "family_name";

		/// <summary>
		/// Middle name(s)		
		/// </summary>
		public const string MiddleName = "middle_name";

		/// <summary>
		/// Casual name		
		/// </summary>
		public const string CasualName = "nickname";

		/// <summary>
		/// Shorthand name by which the End-User wishes to be referred to
		/// </summary>
		public const string PreferredUserName = "preferred_username";

		/// <summary>
		/// Profile page URL
		/// </summary>
		public const string ProfilePageUrl = "profile";

		/// <summary>
		/// Profile picture URL
		/// </summary>
		public const string ProfilePictureUrl = "picture";

		/// <summary>
		/// Web page or blog URL
		/// </summary>
		public const string WebPage = "website";

		/// <summary>
		/// Preferred e-mail address
		/// </summary>
		public const string EMail = "email";

		/// <summary>
		/// True if the e-mail address has been verified; otherwise false		
		/// </summary>
		public const string VerifiedEMail = "email_verified";

		/// <summary>
		/// Gender
		/// </summary>
		public const string Gender = "gender";

		/// <summary>
		/// Birthday
		/// </summary>
		public const string Birthday = "birthdate";

		/// <summary>
		/// Time zone
		/// </summary>
		public const string TimeZone = "zoneinfo";

		/// <summary>
		/// Locale
		/// </summary>
		public const string Locale = "locale";

		/// <summary>
		/// Preferred telephone number
		/// </summary>
		public const string PhoneNumber = "phone_number";

		/// <summary>
		/// True if the phone number has been verified; otherwise false		
		/// </summary>
		public const string VerifiedPhoneNumber = "phone_number_verified";

		/// <summary>
		/// Preferred postal address
		/// </summary>
		public const string Address = "address";

		/// <summary>
		/// Time the information was last updated
		/// </summary>
		public const string UpdatedAt = "updated_at";

		/// <summary>
		/// Authorized party - the party to which the ID Token was issued
		/// </summary>
		public const string AuthorizedParty = "azp";

		/// <summary>
		/// Value used to associate a Client session with an ID Token
		/// </summary>
		public const string Nonce = "nonce";

		/// <summary>
		/// Time when the authentication occurred
		/// </summary>
		public const string AuthenticationTime = "auth_time";

		/// <summary>
		/// Access Token hash value
		/// </summary>
		public const string AccessTokenHash = "at_hash";

		/// <summary>
		/// Code hash value
		/// </summary>
		public const string CodeHash = "c_hash";

		/// <summary>
		/// Authentication Context Class Reference
		/// </summary>
		public const string AuthenticationContextClassReference = "acr";

		/// <summary>
		/// Authentication Methods References
		/// </summary>
		public const string AuthenticationMethodsReferences = "amr";

		/// <summary>
		/// Public key used to check the signature of an ID Token
		/// </summary>
		public const string PublicKey = "sub_jwk";

		/// <summary>
		/// Session ID
		/// </summary>
		public const string SessionId = "sid";
	}
}
