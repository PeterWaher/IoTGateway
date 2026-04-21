using System;
using Waher.Content;
using Waher.Persistence;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contains personal information found in a legal identity.
	/// </summary>
	public class PersonalInformation
	{
		/// <summary>
		/// FIRST
		/// </summary>
		public const string FirstNameTag = "FIRST";

		/// <summary>
		/// MIDDLE
		/// </summary>
		public const string MiddleNamesTag = "MIDDLE";

		/// <summary>
		/// LAST
		/// </summary>
		public const string LastNamesTag = "LAST";

		/// <summary>
		/// FULLNAME
		/// </summary>
		public const string FullNameTag = "FULLNAME";

		/// <summary>
		/// ADDR
		/// </summary>
		public const string AddressTag = "ADDR";

		/// <summary>
		/// ADDR2
		/// </summary>
		public const string Address2Tag = "ADDR2";

		/// <summary>
		/// FULLADDR
		/// </summary>
		public const string FullAddressTag = "FULLADDR";

		/// <summary>
		/// ZIP
		/// </summary>
		public const string PostalCodeTag = "ZIP";

		/// <summary>
		/// AREA
		/// </summary>
		public const string AreaTag = "AREA";

		/// <summary>
		/// CITY
		/// </summary>
		public const string CityTag = "CITY";

		/// <summary>
		/// REGION
		/// </summary>
		public const string RegionTag = "REGION";

		/// <summary>
		/// COUNTRY
		/// </summary>
		public const string CountryTag = "COUNTRY";

		/// <summary>
		/// NATIONALITY
		/// </summary>
		public const string NationalityTag = "NATIONALITY";

		/// <summary>
		/// GENDER
		/// </summary>
		public const string GenderTag = "GENDER";

		/// <summary>
		/// BDAY
		/// </summary>
		public const string BirthDayTag = "BDAY";

		/// <summary>
		/// BMONTH
		/// </summary>
		public const string BirthMonthTag = "BMONTH";

		/// <summary>
		/// BYEAR
		/// </summary>
		public const string BirthYearTag = "BYEAR";

		/// <summary>
		/// AGEABOVE
		/// </summary>
		public const string AgeAboveTag = "AGEABOVE";

		/// <summary>
		/// PNR
		/// </summary>
		public const string PersonalNumberTag = "PNR";

		/// <summary>
		/// ORGNAME
		/// </summary>
		public const string OrganizationNameTag = "ORGNAME";

		/// <summary>
		/// ORGDEPT
		/// </summary>
		public const string OrganizationDepartmentTag = "ORGDEPT";

		/// <summary>
		/// ORGROLE
		/// </summary>
		public const string OrganizationRoleTag = "ORGROLE";

		/// <summary>
		/// ORGADDR
		/// </summary>
		public const string OrganizationAddressTag = "ORGADDR";

		/// <summary>
		/// ORGADDR2
		/// </summary>
		public const string OrganizationAddress2Tag = "ORGADDR2";

		/// <summary>
		/// ORGZIP
		/// </summary>
		public const string OrganizationPostalCodeTag = "ORGZIP";

		/// <summary>
		/// ORGAREA
		/// </summary>
		public const string OrganizationAreaTag = "ORGAREA";

		/// <summary>
		/// ORGCITY
		/// </summary>
		public const string OrganizationCityTag = "ORGCITY";

		/// <summary>
		/// ORGREGION
		/// </summary>
		public const string OrganizationRegionTag = "ORGREGION";

		/// <summary>
		/// ORGCOUNTRY
		/// </summary>
		public const string OrganizationCountryTag = "ORGCOUNTRY";

		/// <summary>
		/// ORGNR
		/// </summary>
		public const string OrganizationNumberTag = "ORGNR";

		/// <summary>
		/// PHONE
		/// </summary>
		public const string PhoneTag = "PHONE";

		/// <summary>
		/// EMAIL
		/// </summary>
		public const string EMailTag = "EMAIL";

		/// <summary>
		/// JID
		/// </summary>
		public const string JidTag = "JID";

		/// <summary>
		/// AGENT
		/// </summary>
		public const string AgentTag = "AGENT";

		/// <summary>
		/// DOMAIN
		/// </summary>
		public const string DomainTag = "DOMAIN";

		/// <summary>
		/// PREVIEW
		/// </summary>
		public const string PreviewTag = "PREVIEW";

		/// <summary>
		/// DEVICE_ID
		/// </summary>
		public const string DeviceIdTag = "DEVICE_ID";

		/// <summary>
		/// First name
		/// </summary>
		public CaseInsensitiveString FirstName = null;

		/// <summary>
		/// Middle names
		/// </summary>
		public CaseInsensitiveString MiddleNames = null;

		/// <summary>
		/// Last names
		/// </summary>
		public CaseInsensitiveString LastNames = null;

		/// <summary>
		/// FullName
		/// </summary>
		public CaseInsensitiveString FullName = null;

		/// <summary>
		/// Address
		/// </summary>
		public CaseInsensitiveString Address = null;

		/// <summary>
		/// Address, second row
		/// </summary>
		public CaseInsensitiveString Address2 = null;

		/// <summary>
		/// Postal Code
		/// </summary>
		public CaseInsensitiveString PostalCode = null;

		/// <summary>
		/// Area
		/// </summary>
		public CaseInsensitiveString Area = null;

		/// <summary>
		/// City
		/// </summary>
		public CaseInsensitiveString City = null;

		/// <summary>
		/// Region
		/// </summary>
		public CaseInsensitiveString Region = null;

		/// <summary>
		/// Country
		/// </summary>
		public CaseInsensitiveString Country = null;

		/// <summary>
		/// Nationality
		/// </summary>
		public CaseInsensitiveString Nationality = null;

		/// <summary>
		/// Personal Number
		/// </summary>
		public CaseInsensitiveString PersonalNumber = null;

		/// <summary>
		/// Organization Name
		/// </summary>
		public CaseInsensitiveString OrgName = null;

		/// <summary>
		/// Organization Department
		/// </summary>
		public CaseInsensitiveString OrgDepartment = null;

		/// <summary>
		/// Role in Organization
		/// </summary>
		public CaseInsensitiveString OrgRole = null;

		/// <summary>
		/// Organization Address
		/// </summary>
		public CaseInsensitiveString OrgAddress = null;

		/// <summary>
		/// Organization Address, second row
		/// </summary>
		public CaseInsensitiveString OrgAddress2 = null;

		/// <summary>
		/// Organization Postal Code
		/// </summary>
		public CaseInsensitiveString OrgPostalCode = null;

		/// <summary>
		/// Organization Area
		/// </summary>
		public CaseInsensitiveString OrgArea = null;

		/// <summary>
		/// Organization City
		/// </summary>
		public CaseInsensitiveString OrgCity = null;

		/// <summary>
		/// Organization Region
		/// </summary>
		public CaseInsensitiveString OrgRegion = null;

		/// <summary>
		/// Organization Country
		/// </summary>
		public CaseInsensitiveString OrgCountry = null;

		/// <summary>
		/// Organization Number
		/// </summary>
		public CaseInsensitiveString OrgNumber = null;

		/// <summary>
		/// e-mail address
		/// </summary>
		public string EMail = null;

		/// <summary>
		/// Phone Number
		/// </summary>
		public string Phone = null;

		/// <summary>
		/// JID
		/// </summary>
		public string Jid = null;

		/// <summary>
		/// Gender
		/// </summary>
		public Gender? Gender = null;

		/// <summary>
		/// Birth year
		/// </summary>
		public int? BirthYear = null;

		/// <summary>
		/// Birth month
		/// </summary>
		public int? BirthMonth = null;

		/// <summary>
		/// Birth day
		/// </summary>
		public int? BirthDay = null;

		/// <summary>
		/// Age above the stated number of years
		/// </summary>
		public int? AgeAbove = null;

		/// <summary>
		/// If identity has organization information
		/// </summary>
		public bool HasOrg = false;

		/// <summary>
		/// If identity has birth date
		/// </summary>
		public bool HasBirthDate = false;

		/// <summary>
		/// Birth date
		/// </summary>
		public DateTime? BirthDate
		{
			get
			{
				if (this.HasBirthDate)
					return new DateTime(this.BirthYear.Value, this.BirthMonth.Value, this.BirthDay.Value);
				else
					return null;
			}
		}

		/// <summary>
		/// Age
		/// </summary>
		public int Age => Duration.GetDurationBetween(this.BirthDate.Value, DateTime.Now).Years;
	}
}
