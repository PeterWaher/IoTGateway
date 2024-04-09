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
		/// Full name
		/// </summary>
		public const string Name = "name";

		/// <summary>
		/// Given name(s) or first name(s)
		/// </summary>
		public const string GivenName = "given_name";

		/// <summary>
		/// Surname(s) or last name(s)
		/// </summary>
		public const string FamilyName = "family_name";

		/// <summary>
		/// Middle name(s)
		/// </summary>
		public const string MiddleName = "middle_name";

		/// <summary>
		/// Casual name
		/// </summary>
		public const string NickName = "nickname";

		/// <summary>
		/// Shorthand name by which the End-User wishes to be referred to
		/// </summary>
		public const string PreferredUserName = "preferred_username";

		/// <summary>
		/// Profile page URL
		/// </summary>
		public const string Profile = "profile";

		/// <summary>
		/// Profile picture URL
		/// </summary>
		public const string Picture = "picture";

		/// <summary>
		/// Web page or blog URL
		/// </summary>
		public const string WebSite = "website";

		/// <summary>
		/// Preferred e-mail address
		/// </summary>
		public const string EMail = "email";

		/// <summary>
		/// True if the e-mail address has been verified; otherwise false
		/// </summary>
		public const string EMailVerified = "email_verified";

		/// <summary>
		/// Gender
		/// </summary>
		public const string Gender = "gender";

		/// <summary>
		/// Birthday
		/// </summary>
		public const string BirthDate = "birthdate";

		/// <summary>
		/// Time zone
		/// </summary>
		public const string ZoneInfo = "zoneinfo";

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
		public const string PhoneNumberVerified = "phone_number_verified";

		/// <summary>
		/// Preferred postal address
		/// </summary>
		public const string Address = "address";

		/// <summary>
		/// Time the information was last updated
		/// </summary>
		public const string UpdatedAt = "updated_at";

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

		/// <summary>
		/// Roles
		/// </summary>
		public const string Roles = "roles";

		/// <summary>
		/// Groups
		/// </summary>
		public const string Groups = "groups";

		/// <summary>
		/// Entitlements
		/// </summary>
		public const string Entitlements = "entitlements";

		/// <summary>
		/// The geographic location
		/// </summary>
		public const string Location = "location";

		/// <summary>
		/// A structured Claim representing the End-User's place of birth.
		/// </summary>
		public const string PlaceOfBirth = "place_of_birth";

		/// <summary>
		/// String array representing the End-User's nationalities.
		/// </summary>
		public const string Nationalities = "nationalities";

		/// <summary>
		/// Family name(s) someone has when they were born, or at least from the time they were a child. This term can be used by a person 
		/// who changes the family name(s) later in life for any reason. Note that in some cultures, people can have multiple family names 
		/// or no family name; all can be present, with the names being separated by space characters.
		/// </summary>
		public const string BirthFamilyName = "birth_family_name";

		/// <summary>
		/// Given name(s) someone has when they were born, or at least from the time they were a child. This term can be used by a person who 
		/// changes the given name later in life for any reason. Note that in some cultures, people can have multiple given names; all can be 
		/// present, with the names being separated by space characters.
		/// </summary>
		public const string BirthGivenName = "birth_given_name";

		/// <summary>
		/// Middle name(s) someone has when they were born, or at least from the time they were a child. This term can be used by a person who 
		/// changes the middle name later in life for any reason. Note that in some cultures, people can have multiple middle names; all can be 
		/// present, with the names being separated by space characters. Also note that in some cultures, middle names are not used.
		/// </summary>
		public const string BirthMiddleName = "birth_middle_name";

		/// <summary>
		/// End-User's salutation, e.g., "Mr."
		/// </summary>
		public const string Salutation = "salutation";

		/// <summary>
		/// End-User's title, e.g., "Dr."
		/// </summary>
		public const string Title = "title";

		/// <summary>
		/// Subject Identifier
		/// </summary>
		public const string SubjectIdentifier = "sub_id";

		/// <summary>
		/// Hardware name (name of device/machine).
		/// </summary>
		public const string HardwareName = "hwname";

		/// <summary>
		/// Model identifier for hardware
		/// </summary>
		public const string HardwareModel = "hwmodel";

		/// <summary>
		/// Hardware Version Identifier
		/// </summary>
		public const string HardwareVersion = "hwversion";

		/// <summary>
		/// The name of the software running in the entity
		/// </summary>
		public const string SoftwareName = "swname";

		/// <summary>
		/// The version of software running in the entity
		/// </summary>
		public const string SoftwareVersion = "swversion";
	}
}
