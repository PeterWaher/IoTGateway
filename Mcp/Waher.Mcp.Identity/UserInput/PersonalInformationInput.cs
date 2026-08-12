using System;
using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Identity.UserInput
{
	internal enum Gender
	{
		/// <summary>
		/// Male
		/// </summary>
		M,

		/// <summary>
		/// Female
		/// </summary>
		F
	}

	/// <summary>
	/// Class containing user input parameters for a personal identity application
	/// </summary>
	internal class PersonalInformationInput
	{
		/// <summary>
		/// First name of the user.
		/// </summary>
		[McpStringParameter("First Name", "First name of the user.", 1, 128)]
		public string FirstName = string.Empty;

		/// <summary>
		/// Middle names of the user.
		/// </summary>
		[McpStringParameter("Middle Names", "Middle names of the user.", 0, 128)]
		public string? MiddleNames = null;

		/// <summary>
		/// Last names of the user.
		/// </summary>
		[McpStringParameter("Last Names", "Last names of the user.", 1, 128)]
		public string LastNames = string.Empty;

		/// <summary>
		/// Personal number of the user. Must conform to the personal numbering scheme 
		/// defined in the country of residence.
		/// </summary>
		[McpStringParameter("Personal Number", "Personal number of the user. Must " +
			"conform to the personal numbering scheme defined in the country of " +
			"residence.",
			1, 128)]
		public string PersonalNumber = string.Empty;

		/// <summary>
		/// Address where the user is resident.
		/// </summary>
		[McpStringParameter("Address", "Address where the user is resident.", 1, 128)]
		public string Address = string.Empty;

		/// <summary>
		/// Second Address line where the user is resident.
		/// </summary>
		[McpStringParameter("Address 2nd line", "Second Address line where the user is resident.", 0, 128)]
		public string? Address2 = null;

		/// <summary>
		/// Zip or postal code where the user is resident.
		/// </summary>
		[McpStringParameter("Zip", "Zip or postal code where the user is resident.", 1, 128)]
		public string Zip = string.Empty;

		/// <summary>
		/// Area where the user is resident.
		/// </summary>
		[McpStringParameter("Area", "Area where the user is resident.", 0, 128)]
		public string? Area = null;

		/// <summary>
		/// City where the user is resident.
		/// </summary>
		[McpStringParameter("City", "City where the user is resident.", 1, 128)]
		public string City = string.Empty;

		/// <summary>
		/// Region where the user is resident.
		/// </summary>
		[McpStringParameter("Region", "Region where the user is resident.", 0, 128)]
		public string? Region = null;

		/// <summary>
		/// Country where the user is resident. Must conform to ISO 3166.
		/// </summary>
		[McpStringParameter("Country", "Country where the user is resident. Must conform to ISO 3166.", 2, 2)]
		public string Country = string.Empty;

		/// <summary>
		/// Nationality of the user. Must conform to ISO 3166.
		/// </summary>
		[McpStringParameter("Nationality", "Nationality of the user. Must conform to ISO 3166.", 2, 2)]
		public string? Nationality = null;

		/// <summary>
		/// Birth date of the user.
		/// </summary>
		[McpDateParameter("Birth Date", "Birth date of the user.")]
		public DateTime? BirthDate = null;

		/// <summary>
		/// Gender of the user.
		/// </summary>
		[McpParameter("Gender", "Gender of the user.")]
		[McpEnumValue(UserInput.Gender.M, "Male")]
		[McpEnumValue(UserInput.Gender.F, "Female")]
		public Gender? Gender = null;
	}
}
